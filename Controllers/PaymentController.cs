using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using checkout.Models;
using Checkout.utils;
using Braintree;

namespace checkout.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private IConfiguration _configuration;

        public PaymentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("initiate")]
        public JsonResult initiatePayment()
        {
            BraintreeGateway braintreeGateway = BraintreeUtil.CreateGateway();
            var token = braintreeGateway.ClientToken.Generate();
            return new JsonResult(token);
        }

        [HttpPost]
        [Route("submit")]
        public JsonResult submitPayment([FromForm] string nonce, [FromForm] int invoiceId)
        {
            string query = "select id, amount, status from dbo.invoice where id = @invoiceId";
            DataTable table = new DataTable();
            string databaseConnection = _configuration.GetConnectionString("checkoutDBConnection");
            SqlDataReader dataReader;
            using (SqlConnection connection = new SqlConnection(databaseConnection))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@invoiceId", invoiceId);
                    dataReader = command.ExecuteReader();
                    table.Load(dataReader);
                    dataReader.Close();
                    connection.Close();
                }
            }

            Invoice invoice = getInvoices(table)[0];

            var request = new TransactionRequest
            {
                Amount = invoice.amount,
                PaymentMethodNonce = nonce,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };
            BraintreeGateway braintreeGateway = BraintreeUtil.CreateGateway();
            Result<Transaction> result = braintreeGateway.Transaction.Sale(request);
            if (result.IsSuccess())
            {
                string updateQuery = "update dbo.invoice set status = 'COMPLETED' where id = @invoiceId";
                databaseConnection = _configuration.GetConnectionString("checkoutDBConnection");
                using (SqlConnection connection = new SqlConnection(databaseConnection))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@invoiceId", invoiceId);
                        dataReader = command.ExecuteReader();
                        dataReader.Close();
                        connection.Close();
                    }
                }
                return new JsonResult("Payment Submitted Successfully");
            }
            else
            {
                string errorMessages = "";
                foreach (ValidationError error in result.Errors.DeepAll())
                {
                    errorMessages += "Error: " + (int)error.Code + " - " + error.Message + "\n";
                }
                Console.WriteLine(result.Message);
                return new JsonResult(errorMessages);
            }
        }

        private List<Invoice> getInvoices(DataTable table)
        {

            var invoices = (from rw in table.AsEnumerable()
                            select new Invoice()
                            {
                                id = Convert.ToInt32(rw["id"]),
                                amount = Convert.ToInt32(rw["amount"]),
                                status = Convert.ToString(rw["status"])
                            }).ToList();

            return invoices;
        }
    }
}
