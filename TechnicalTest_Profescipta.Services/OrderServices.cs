using Azure.Core;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using TechnicalTest_Profescipta.Common.DTO;
using TechnicalTest_Profescipta.Common.Entity;
using TechnicalTest_Profescipta.Common.Interface;
using TechnicalTest_Profescipta.DAL.Context;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TechnicalTest_Profescipta.Services
{
    public class OrderServices : IOrderServices
    {
        public async Task<SalesOrder> getbyId(long id, CancellationToken cancellationToken)
        {
            using DBContext dbContext = new DBContext();
            try
            {

                var result = await dbContext.SoOrders
                        .Select(order => new SalesOrder
                        {
                            SoOrderId = order.SoOrderId,
                            OrderNo = order.OrderNo,
                            OrderDate = order.OrderDate,
                            ComCustomerId = order.ComCustomerId,
                            CustomerName = dbContext.ComCustomers
                                .Where(c => c.ComCustomerId == order.ComCustomerId)
                                .Select(c => c.CustomerName)
                                .FirstOrDefault(),
                            Address = order.Address,
                            Items = dbContext.SoItems.Where(v => v.SoOrderId == order.SoOrderId).Select(item => new SalesOrderItem
                            {
                                SoOrderId = item.SoOrderId,
                                SoItemId = item.SoItemId,
                                ItemName = item.ItemName,
                                Quantity = item.Quantity,
                                Price = item.Price,
                            }).ToList(),
                        })
                        .FirstOrDefaultAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                await dbContext.DisposeAsync();
            }
        }
        public async Task<DatatablesResponse<List<SalesOrder>>> datalist(DatatableRequestOrder request, CancellationToken cancellationToken)
        {
            using DBContext dbContext = new DBContext();
            try
            {
                var result = new DatatablesResponse<List<SalesOrder>>();
                string txtSearch = request.search.value.ToLower() ?? "";

                var query = (from order in dbContext.SoOrders
                             join costemer in dbContext.ComCustomers on order.ComCustomerId equals costemer.ComCustomerId
                             select new SalesOrder
                             {
                                 SoOrderId = order.SoOrderId,
                                 OrderNo = order.OrderNo,
                                 OrderDate = order.OrderDate,
                                 ComCustomerId = order.ComCustomerId,
                                 CustomerName = costemer.CustomerName,
                                Address = order.Address,
                             }).AsQueryable();

                if (!string.IsNullOrEmpty(txtSearch))
                {
                    query = query.Where(a => a.OrderNo.Equals(txtSearch) || a.CustomerName.Equals(txtSearch)).AsQueryable();
                }

                if (request.orderDate.HasValue)
                {
                    query = query.Where(a => a.OrderDate == request.orderDate).AsQueryable();
                }

                var finalData = await query.OrderBy(a => a.OrderNo).Skip(request.start * request.length).Take(request.length).ToListAsync(cancellationToken);

                result.data = finalData;
                result.draw = request.draw;
                result.recordsTotal = query.Count();
                result.recordsFiltered = query.Count();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                await dbContext.DisposeAsync();
            }
        }

        public async Task<Boolean> Delete(long orderId, CancellationToken cancellationToken)
        {
            using DBContext dbContext = new DBContext();
            using var trans = dbContext.Database.BeginTransaction();
            try
            {
                var check = await  dbContext.SoOrders.FirstOrDefaultAsync(a => a.SoOrderId == orderId, cancellationToken);

                if (check == null) throw new Exception("Data Order Not Found");

                var checkItem = await  dbContext.SoItems.Where(a => a.SoOrderId == orderId).ToListAsync(cancellationToken);

                dbContext.SoItems.RemoveRange(checkItem);
                dbContext.SoOrders.Remove(check);

                var result = await dbContext.SaveChangesAsync(cancellationToken) > 0;

                if (!result) throw new Exception("Failed to delete Order");

                await trans.CommitAsync(cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                await trans.DisposeAsync();
                await dbContext.DisposeAsync();
            }
        }

        public async Task<SalesOrder> SaveOrUpdate(SalesOrder request, CancellationToken cancellationToken)
        {
            using DBContext dbContext = new DBContext();
            using var trans = dbContext.Database.BeginTransaction();
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));

                SalesOrder response = new();

                if(request.SoOrderId == 0)
                {
                    response = await save(request, dbContext, cancellationToken);
                }
                else
                {
                    response = await Update(request, dbContext, cancellationToken);
                }


                int number =  1;
                List<long> updatedItemIds = new List<long>();
                foreach (SalesOrderItem item in request.Items)
                {
                    var responseItem = new SalesOrderItem();
                    if(item.SoItemId == 0)
                    {
                        responseItem = await saveItem(item, response.SoOrderId,dbContext,cancellationToken);
                    }
                    else
                    {
                        responseItem = await UpdateItem(item, dbContext,cancellationToken);
                    }
                    responseItem.no = number++;
                    response.Items.Add(responseItem);
                    updatedItemIds.Add(responseItem.SoItemId);
                }

                var existingItems = await dbContext.SoItems.Where(x => x.SoOrderId == response.SoOrderId).ToListAsync(cancellationToken);

                var itemsToDelete = existingItems.Where(x => !updatedItemIds.Contains(x.SoItemId)).ToList();

                foreach (var item in itemsToDelete)
                {
                    dbContext.SoItems.Remove(item);
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                await trans.CommitAsync(cancellationToken);
                return response;
            }catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                await trans.DisposeAsync();
                await dbContext.DisposeAsync();
            }
        }

        private async Task<SalesOrder> save(SalesOrder request, DBContext dbContext,CancellationToken cancellationToken)
        {
            SoOrder payload = new SoOrder
            {
                OrderNo = request.OrderNo,
                OrderDate = request.OrderDate.Value,
                ComCustomerId = request.ComCustomerId,
                Address = request.Address,
            };

            dbContext.SoOrders.Add(payload);
            var result = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            if (!result) throw new Exception("Failed to Insert Order");

            return new SalesOrder{ 
                SoOrderId = payload.SoOrderId,
                OrderNo = payload.OrderNo,
                OrderDate = payload.OrderDate,
                ComCustomerId = payload.ComCustomerId,
                Address = payload.Address,
            };
        }

        private async Task<SalesOrder> Update(SalesOrder request, DBContext dbContext, CancellationToken cancellationToken)
        {
            var check = await dbContext.SoOrders.FirstOrDefaultAsync((a) => a.SoOrderId == request.SoOrderId, cancellationToken);

            if (check == null) throw new Exception("Data Not Found");

            check.OrderNo = request.OrderNo;
            check.OrderDate = request.OrderDate.Value;
            check.ComCustomerId = request.ComCustomerId;
            check.Address = request.Address;

            dbContext.SoOrders.Update(check);
            var result = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            if (!result) throw new Exception("Failed to Update Order");

            return new SalesOrder
            {
                SoOrderId = check.SoOrderId,
                OrderNo = check.OrderNo,
                OrderDate = check.OrderDate,
                ComCustomerId = check.ComCustomerId,
                Address = check.Address,
            };
        }

        public async Task<Stream> export(CancellationToken cancellationToken)
        {
            MemoryStream stream = new MemoryStream();
            using var context = new DBContext();
            try
            {
                List<SalesOrder> data = await (from order in context.SoOrders
                                                           join costemer in context.ComCustomers on order.ComCustomerId equals costemer.ComCustomerId
                                                           select new SalesOrder
                                                           {
                                                               SoOrderId = order.SoOrderId,
                                                               OrderNo = order.OrderNo,
                                                               OrderDate = order.OrderDate,
                                                               ComCustomerId = order.ComCustomerId,
                                                               CustomerName = costemer.CustomerName,
                                                               Address = order.Address,
                                                           }).ToListAsync(cancellationToken);

                using (var package = new ExcelPackage(stream))
                {
                    var Sheet = package.Workbook.Worksheets.Add("Sales Order");

                    Sheet.Cells[1, 1].Value = "No";
                    Sheet.Cells[1, 2].Value = "Order No";
                    Sheet.Cells[1, 3].Value = "Order Date";
                    Sheet.Cells[1, 4].Value = "Costemer Name";
                    Sheet.Cells[1, 5].Value = "Address";

                    int row = 2;
                    int no = 1;

                    foreach (var item in data)
                    {
                        Sheet.Cells[row, 1].Value = no;
                        Sheet.Cells[row, 2].Value = item.OrderNo;
                        Sheet.Cells[row, 3].Value = item.OrderDate?.ToString("dd/MM/yyyy");
                        Sheet.Cells[row, 4].Value = item.CustomerName;
                        Sheet.Cells[row, 5].Value = item.Address;

                        row++;
                        no++;
                    }

                    var border = Sheet.Cells[1, 1, row, 5].Style.Border;
                    border.Top.Style = ExcelBorderStyle.Thin;
                    border.Bottom.Style = ExcelBorderStyle.Thin;
                    border.Left.Style = ExcelBorderStyle.Thin;
                    border.Right.Style = ExcelBorderStyle.Thin;

                    Sheet.Cells[1, 1, row, 5].AutoFitColumns();
                    package.Workbook.Properties.Title = "Export Sales Order";

                    await package.SaveAsync();
                }


                stream.Position = 0;
                return stream;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                await context.DisposeAsync();
            }
        }

        //ITEMS
        #region ITEMS

        private async Task<SalesOrderItem> saveItem(SalesOrderItem request, long OrderId, DBContext dbContext, CancellationToken cancellationToken)
        {
            SoItem payload = new SoItem
            {
                SoOrderId = OrderId,
                ItemName = request.ItemName,
                Quantity = request.Quantity,
                Price = request.Price,
            };

            dbContext.SoItems.Add(payload);
            var result = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            if (!result) throw new Exception($"Failed to Insert Item {payload.ItemName}");

            return new SalesOrderItem
            {
                SoOrderId = payload.SoOrderId,
                SoItemId = payload.SoItemId,
                ItemName = payload.ItemName,
                Quantity = payload.Quantity,
                Price = payload.Price
            };
        }
        private async Task<SalesOrderItem> UpdateItem(SalesOrderItem request, DBContext dbContext, CancellationToken cancellationToken)
        {
            var check = await dbContext.SoItems.FirstOrDefaultAsync((a) => a.SoItemId == request.SoItemId, cancellationToken);

            if (check == null) throw new Exception("Data Not Found");

            check.ItemName = request.ItemName;
            check.Quantity = request.Quantity;
            check.Price = request.Price;

            dbContext.SoItems.Update(check);
            var result = await dbContext.SaveChangesAsync(cancellationToken) > 0;

            if (!result) throw new Exception($"Failed to Insert Item {check.ItemName}");

            return new SalesOrderItem
            {
                SoOrderId = check.SoOrderId,
                SoItemId = check.SoItemId,
                ItemName = check.ItemName,
                Quantity = check.Quantity,
                Price = check.Price
            };
        }

        #endregion
    }
}
