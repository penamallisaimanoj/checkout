> /api/Invoice/invoices

Retrieve existing invoices along with status.

> /api/Payment/initiate

Intiate the Payment capture which returns a unique token generated with merchantId and secrets

> /api/Payment/submit

Submit payment details along with generated nonce by client to Braintree and return the status of the transaction.
(Failing with Error: Error: 91565 - Unknown or expired payment_method_nonce.)
