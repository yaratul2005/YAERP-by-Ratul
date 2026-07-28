import re

with open('tests/YAERP.Application.Tests/Sales/SalesCommandTests.cs', 'r') as f:
    sales_test = f.read()

sales_test = sales_test.replace(
    'var idProp = customer.GetType().GetProperty("Id"); if(idProp != null && idProp.CanWrite) { idProp.SetValue(customer, new YAERP.Domain.Sales.CustomerId(customerId)); } else { typeof(YAERP.Domain.Common.Entity<YAERP.Domain.Sales.CustomerId>).GetProperty("Id")?.SetValue(customer, new YAERP.Domain.Sales.CustomerId(customerId)); }',
    'typeof(YAERP.Domain.Common.Primitives.Entity<YAERP.Domain.Sales.CustomerId>).GetProperty("Id")?.SetValue(customer, new YAERP.Domain.Sales.CustomerId(customerId));'
)
with open('tests/YAERP.Application.Tests/Sales/SalesCommandTests.cs', 'w') as f:
    f.write(sales_test)
