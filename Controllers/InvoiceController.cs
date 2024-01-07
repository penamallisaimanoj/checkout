using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace checkout.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private IConfiguration _configuration;

        public InvoiceController(IConfiguration configuration) {
            _configuration = configuration;
        }

        [HttpGet]
        [Route("invoices")]
        public JsonResult getInvoices()
        {
            string query = "select * from dbo.invoice";
            DataTable table = new DataTable();
            string databaseConnection = _configuration.GetConnectionString("checkoutDBConnection");
            SqlDataReader dataReader;
            using(SqlConnection connection = new SqlConnection(databaseConnection))
            {
                connection.Open();
                using(SqlCommand command = new SqlCommand(query, connection))
                {
                    dataReader = command.ExecuteReader();
                    table.Load(dataReader);
                    dataReader.Close();
                    connection.Close();
                }
            }
            return new JsonResult(table);
        }
    }
}
