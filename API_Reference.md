# AutoEase Backend API Reference

This document is rebuilt from the current controller code in `Controllers/` and related DTO/entity classes. It reflects the routes and payload shapes that are actually implemented right now.

## Base URLs

- Development HTTP: `http://localhost:5209`
- Development HTTPS: `https://localhost:7193`

## Authentication

Protected endpoints expect this header:

```http
Authorization: Bearer <jwt-token>
```

JWT tokens are returned by `POST /api/auth/login`.

## Response format notes

- ASP.NET Core serializes JSON in `camelCase`, so frontend code should read fields like `vendorId`, `staffId`, `phoneNumber`, and `invoiceItems`.
- Some endpoints return full EF entities, some return trimmed custom objects, and some return plain strings. This reference shows the actual behavior.
- Date values are returned as ISO strings.

## Important implementation notes

1. `AdminController`, `VendorController`, `PartsController`, `StaffCustomersController`, and `InvoiceController` do not currently have `[Authorize]` on them. Even if the route name sounds admin-only, the endpoint is public in the current code.
2. Role casing is inconsistent in the code:
   - public register stores role as `Customer`
   - `AdminController.CreateStaff` stores `staff`
   - `AdminController.UpdateRole` stores lowercase values like `admin`, `staff`, `customer`
   - some protected endpoints require `Customer`, `Staff`, or `Admin`
3. There are two invoice APIs:
   - `api/invoices` for staff/admin-created customer invoices
   - `api/invoice` for purchase/sales invoice flows via services
4. `POST /api/invoices` uses `customerId` as a query string, not in the JSON body.
5. `POST /api/invoices` receives `unitPrice` in each item only to calculate totals. That value is not stored in `InvoiceItem`.

## Frontend integration guide

### Login and token usage

1. Call `POST /api/auth/login`.
2. Save `token`.
3. Send `Authorization: Bearer <token>` on protected routes.
4. If you need user info after login, call `GET /api/profile`.

### Booking an appointment

To create an appointment, the frontend needs `vehicleId` and `staffId`.

1. Call `GET /api/vehicles` after customer login.
2. Render the returned vehicles in a `<select>` and submit the chosen `id` as `vehicleId`.
3. Call `GET /api/admin/staff-list` to fetch available staff IDs.
4. Render that list in another `<select>` and submit the chosen `id` as `staffId`.
5. Send `POST /api/appointments`.

### Creating an invoice

To create an invoice with `POST /api/invoices`, the frontend needs `customerId`, `vendorId`, `staffId`, and one or more `partId` values.

1. Search customer first with `GET /api/staff/customers/search?query=...`.
2. Render search results and keep the chosen customer's `id`.
3. Fetch vendors from `GET /api/vendor/list` and render them in a vendor dropdown. Submit the selected vendor `id` as `vendorId`.
4. Fetch parts from `GET /api/parts` and render them in a parts table or multi-row form. Each selected part contributes a `partId`, `quantity`, and `unitPrice`.
5. Fetch staff from `GET /api/admin/staff-list` if the staff member must be selected manually. Otherwise, if your UI uses the logged-in staff member automatically, use that ID.
6. Submit the request to `POST /api/invoices?customerId={customerId}`.

### Creating or updating a part

`Part.vendorId` and `Part.requestedBy` are numeric fields in the body.

1. Load vendor options from `GET /api/vendor/list`.
2. Render vendors in a dropdown and submit the selected `id` as `vendorId`.
3. `requestedBy` should be a user ID. If this is staff-driven, use the appropriate staff or requester ID your UI already knows.

### Customer registration by staff

`POST /api/staff/customers` creates both:

- a new user with role `Customer`
- a linked vehicle record

So the frontend should use this endpoint when staff are adding a new customer and first vehicle together.

## Endpoint reference

## AuthController

Base route: `api/auth`

### POST `/api/auth/register`

- Auth: None
- Purpose: Public customer registration
- Body:

```json
{
  "name": "Aayush Sharma",
  "email": "aayush@example.com",
  "password": "StrongPass123!",
  "phone": "9812345678",
  "role": "Customer"
}
```

- Notes:
  - `role` from the request is ignored here.
  - Backend always stores the new user as `Customer`.
  - Email confirmation is required before login.

- Success response:

```json
{
  "message": "Registration successful. Please check your email to verify your account."
}
```

- Common error response:

```json
[
  {
    "code": "DuplicateUserName",
    "description": "Username 'aayush@example.com' is already taken."
  }
]
```

### POST `/api/auth/admin/register-user`

- Auth: Bearer token with role `Admin`
- Purpose: Admin-created user registration
- Body:

```json
{
  "name": "Sita Staff",
  "email": "sita.staff@example.com",
  "password": "StrongPass123!",
  "phone": "9800000001",
  "role": "Staff"
}
```

- Success response:

```json
{
  "message": "User registered successfully by admin."
}
```

### POST `/api/auth/secret-admin-register?secretKey=randomstring`

- Auth: None
- Purpose: Bootstrap an admin account using the configured secret key
- Query params:
  - `secretKey`: must match `AdminSecretKey` in configuration
- Body:

```json
{
  "name": "Main Admin",
  "email": "admin@example.com",
  "password": "StrongPass123!",
  "phone": "9800000000",
  "role": "AnythingHereIsIgnored"
}
```

- Success response:

```json
{
  "message": "Admin registered successfully and email auto-verified."
}
```

- Failure response:

```json
"Invalid secret key."
```

### GET `/api/auth/verify-email?token={token}&email={email}`

- Auth: None
- Purpose: Email confirmation link target
- Query params:
  - `token`
  - `email`

- Success response:

```json
"Email verified successfully"
```

- Failure responses:

```json
"User not found"
```

```json
"Email verification failed"
```

### POST `/api/auth/login`

- Auth: None
- Body:

```json
{
  "email": "aayush@example.com",
  "password": "StrongPass123!"
}
```

- Success response:

```json
{
  "token": "<jwt-token>"
}
```

- Failure responses:

```json
"Please verify your email to log in"
```

```json
"Invalid credentials"
```

## ProfileController

Base route: `api/profile`

### GET `/api/profile`

- Auth: Bearer token
- Success response:

```json
{
  "id": 12,
  "name": "Aayush Sharma",
  "email": "aayush@example.com",
  "phoneNumber": "9812345678",
  "role": "Customer"
}
```

### PUT `/api/profile`

- Auth: Bearer token
- Body:

```json
{
  "name": "Aayush K. Sharma",
  "phone": "9801112233"
}
```

- Success response:

```json
{
  "message": "Profile updated successfully."
}
```

- Validation failure example:

```json
[
  {
    "code": "SomeIdentityError",
    "description": "..."
  }
]
```

## VehiclesController

Base route: `api/vehicles`

### GET `/api/vehicles`

- Auth: Bearer token
- Purpose: Get logged-in user's vehicles
- Success response:

```json
[
  {
    "id": 5,
    "model": "Hyundai i20",
    "plateNumber": "BA-2-CHA-1234"
  }
]
```

- Frontend note:
  - Use this response to populate the vehicle dropdown before calling `POST /api/appointments`.

### POST `/api/vehicles`

- Auth: Bearer token
- Body:

```json
{
  "model": "Honda City",
  "plateNumber": "BA-4-PA-6789"
}
```

- Success response:

```json
{
  "message": "Vehicle added successfully.",
  "id": 9
}
```

### PUT `/api/vehicles/{id}`

- Auth: Bearer token
- Route params:
  - `id`: vehicle ID
- Body:

```json
{
  "model": "Honda City 2020",
  "plateNumber": "BA-4-PA-6789"
}
```

- Success response:

```json
{
  "message": "Vehicle updated successfully."
}
```

### DELETE `/api/vehicles/{id}`

- Auth: Bearer token
- Route params:
  - `id`: vehicle ID
- Success response:

```json
{
  "message": "Vehicle removed successfully."
}
```

## AppointmentsController

Base route: `api/appointments`

### POST `/api/appointments`

- Auth: Bearer token with role `Customer`
- Body:

```json
{
  "vehicleId": 5,
  "staffId": 3,
  "scheduledAt": "2026-05-20T10:30:00Z"
}
```

- Success response:

```json
{
  "id": 11,
  "customerId": 12,
  "vehicleId": 5,
  "staffId": 3,
  "scheduledAt": "2026-05-20T10:30:00Z",
  "status": "Pending",
  "customer": null,
  "staff": null,
  "vehicle": null
}
```

- Frontend note:
  - `vehicleId` should come from `GET /api/vehicles`.
  - `staffId` should come from `GET /api/admin/staff-list`.

### GET `/api/appointments/my-appointments`

- Auth: Bearer token with role `Customer`
- Success response:

```json
[
  {
    "id": 11,
    "customerId": 12,
    "vehicleId": 5,
    "staffId": 3,
    "scheduledAt": "2026-05-20T10:30:00Z",
    "status": "Pending",
    "customer": null,
    "staff": null,
    "vehicle": null
  }
]
```

## PartRequestsController

Base route: `api/partrequests`

### POST `/api/partrequests`

- Auth: Bearer token with role `Customer`
- Body:

```json
{
  "partName": "Brake Pad Set"
}
```

- Success response:

```json
{
  "id": 4,
  "customerId": 12,
  "partName": "Brake Pad Set",
  "status": "Requested",
  "customer": null
}
```

## ReviewsController

Base route: `api/reviews`

### POST `/api/reviews`

- Auth: Bearer token with role `Customer`
- Body:

```json
{
  "rating": 5,
  "comment": "Very good service and quick delivery."
}
```

- Success response:

```json
{
  "id": 7,
  "customerId": 12,
  "rating": 5,
  "comment": "Very good service and quick delivery.",
  "customer": null
}
```

### GET `/api/reviews/my-reviews`

- Auth: Bearer token with role `Customer`
- Purpose: Fetch all reviews created by the logged-in customer
- Success response:

```json
[
  {
    "id": 7,
    "customerId": 12,
    "rating": 5,
    "comment": "Very good service and quick delivery.",
    "customer": null
  }
]
```

### GET `/api/reviews/all`

- Auth: Bearer token with role `Admin`
- Purpose: Fetch all reviews in the system (admin view)
- Success response:

```json
[
  {
    "id": 7,
    "customerId": 12,
    "rating": 5,
    "comment": "Very good service and quick delivery.",
    "customer": null
  }
]
```

## VendorController

Base route: `api/vendor`

### POST `/api/vendor/create`

- Auth: None in current implementation
- Body:

```json
{
  "name": "Everest Auto Parts",
  "phone": "9801112222"
}
```

- Success response:

```json
{
  "message": "Vendor created successfully.",
  "vendor": {
    "id": 2,
    "name": "Everest Auto Parts",
    "phone": "9801112222"
  }
}
```

### GET `/api/vendor/list`

- Auth: None in current implementation
- Success response:

```json
[
  {
    "id": 1,
    "name": "AutoHub Supplies",
    "phone": "9800001000"
  },
  {
    "id": 2,
    "name": "Everest Auto Parts",
    "phone": "9801112222"
  }
]
```

- Frontend note:
  - Use this endpoint to populate vendor dropdowns before creating parts or invoices.

### GET `/api/vendor/{id}`

- Auth: None in current implementation
- Route params:
  - `id`: vendor ID
- Success response:

```json
{
  "id": 2,
  "name": "Everest Auto Parts",
  "phone": "9801112222"
}
```

### PUT `/api/vendor/update/{id}`

- Auth: None in current implementation
- Route params:
  - `id`: vendor ID
- Body:

```json
{
  "name": "Everest Auto Parts Pvt Ltd",
  "phone": "9801112222"
}
```

- Success response:

```json
{
  "message": "Vendor updated successfully.",
  "vendor": {
    "id": 2,
    "name": "Everest Auto Parts Pvt Ltd",
    "phone": "9801112222"
  }
}
```

### DELETE `/api/vendor/delete/{id}`

- Auth: None in current implementation
- Route params:
  - `id`: vendor ID
- Success response:

```json
{
  "message": "Vendor deleted successfully."
}
```

## PartsController

Base route: `api/parts`

### GET `/api/parts`

- Auth: None in current implementation
- Success response:

```json
[
  {
    "id": 1,
    "vendorId": 2,
    "requestedBy": 3,
    "name": "Oil Filter",
    "unitPrice": 450.0,
    "stockQuantity": 40,
    "requestStatus": "Approved",
    "requestDescription": "Routine stock item"
  }
]
```

- Frontend note:
  - Use this endpoint to populate part selection lists before creating invoices.

### GET `/api/parts/{id}`

- Auth: None in current implementation
- Route params:
  - `id`: part ID
- Success response:

```json
{
  "id": 1,
  "vendorId": 2,
  "requestedBy": 3,
  "name": "Oil Filter",
  "unitPrice": 450.0,
  "stockQuantity": 40,
  "requestStatus": "Approved",
  "requestDescription": "Routine stock item"
}
```

### POST `/api/parts`

- Auth: None in current implementation
- Body:

```json
{
  "vendorId": 2,
  "requestedBy": 3,
  "name": "Air Filter",
  "unitPrice": 600.0,
  "stockQuantity": 25,
  "requestStatus": "Pending",
  "requestDescription": "Needed for upcoming service jobs"
}
```

- Success response:

```json
{
  "id": 8,
  "vendorId": 2,
  "requestedBy": 3,
  "name": "Air Filter",
  "unitPrice": 600.0,
  "stockQuantity": 25,
  "requestStatus": "Pending",
  "requestDescription": "Needed for upcoming service jobs",
  "vendor": null,
  "requester": null,
  "invoiceItems": null
}
```

- Frontend note:
  - Load vendor options from `GET /api/vendor/list`.
  - Store the selected vendor `id` as `vendorId`.

### PUT `/api/parts/{id}`

- Auth: None in current implementation
- Route params:
  - `id`: part ID
- Body:

```json
{
  "vendorId": 2,
  "requestedBy": 3,
  "name": "Air Filter Premium",
  "unitPrice": 650.0,
  "stockQuantity": 30,
  "requestStatus": "Approved",
  "requestDescription": "Stock updated after vendor confirmation"
}
```

- Success response:

```json
"Part updated successfully."
```

- Frontend note:
  - This update endpoint overwrites all tracked scalar fields, so send the full part object, not just changed fields.

### DELETE `/api/parts/{id}`

- Auth: None in current implementation
- Route params:
  - `id`: part ID
- Success response:

```json
"Part deleted successfully."
```

## StaffCustomersController

Base route: `api/staff/customers`

### POST `/api/staff/customers`

- Auth: None in current implementation
- Purpose: Create a customer and their first vehicle together
- Body:

```json
{
  "name": "Nabin Karki",
  "email": "nabin@example.com",
  "password": "StrongPass123!",
  "phone": "9811111111",
  "vehicleModel": "Suzuki Swift",
  "plateNumber": "BA-6-CHA-4321"
}
```

- Success response:

```json
{
  "id": 15,
  "name": "Nabin Karki",
  "email": "nabin@example.com",
  "phone": "9811111111",
  "role": "Customer",
  "vehicles": [
    {
      "id": 10,
      "model": "Suzuki Swift",
      "plateNumber": "BA-6-CHA-4321"
    }
  ],
  "appointments": [],
  "invoices": [],
  "vehicleUsageLogs": [],
  "partRequests": [],
  "reviews": []
}
```

- Conflict response examples:

```json
"A customer with email 'nabin@example.com' already exists."
```

```json
"A vehicle with plate number 'BA-6-CHA-4321' already exists."
```

### GET `/api/staff/customers/{customerId}`

- Auth: None in current implementation
- Route params:
  - `customerId`: customer/user ID
- Success response:

```json
{
  "id": 15,
  "name": "Nabin Karki",
  "email": "nabin@example.com",
  "phone": "9811111111",
  "role": "Customer",
  "vehicles": [
    {
      "id": 10,
      "model": "Suzuki Swift",
      "plateNumber": "BA-6-CHA-4321"
    }
  ],
  "appointments": [
    {
      "id": 11,
      "vehicleId": 10,
      "vehicleModel": "Suzuki Swift",
      "vehiclePlateNumber": "BA-6-CHA-4321",
      "staffId": 3,
      "staffName": "Sita Staff",
      "scheduledAt": "2026-05-20T10:30:00Z",
      "status": "Pending"
    }
  ],
  "invoices": [
    {
      "id": 21,
      "vendorId": 2,
      "vendorName": "Everest Auto Parts",
      "staffId": 3,
      "staffName": "Sita Staff",
      "type": "Service",
      "totalAmount": 1800.0,
      "discountApplied": 0.0,
      "paymentStatus": "Pending",
      "invoiceDate": "2026-05-19T09:15:00Z",
      "dueDate": "2026-05-30T00:00:00Z"
    }
  ],
  "vehicleUsageLogs": [],
  "partRequests": [
    {
      "id": 4,
      "partName": "Brake Pad Set",
      "status": "Requested"
    }
  ],
  "reviews": [
    {
      "id": 7,
      "rating": 5,
      "comment": "Very good service and quick delivery."
    }
  ]
}
```

### GET `/api/staff/customers/search?query={query}`

- Auth: None in current implementation
- Query params:
  - `query`: required; can be customer ID, name, email, phone, or plate number fragment
- Success response:

```json
[
  {
    "id": 15,
    "name": "Nabin Karki",
    "email": "nabin@example.com",
    "phone": "9811111111",
    "vehicles": [
      {
        "id": 10,
        "model": "Suzuki Swift",
        "plateNumber": "BA-6-CHA-4321"
      }
    ]
  }
]
```

- Frontend note:
  - This is the best starting point when an invoice flow needs `customerId`.
  - Render results in a searchable table and keep the selected `id`.

## AdminController

Base route: `api/admin`

Current auth note: despite the name, only `POST /api/auth/admin/register-user` is explicitly admin-protected. These `api/admin/*` endpoints do not currently have `[Authorize]`.

### POST `/api/admin/create-staff`

- Auth: None in current implementation
- Body:

```json
{
  "name": "Sita Staff",
  "email": "sita.staff@example.com",
  "password": "StrongPass123!",
  "phone": "9800000001"
}
```

- Success response:

```json
{
  "message": "Staff created successfully.",
  "staff": {
    "id": 3,
    "name": "Sita Staff",
    "email": "sita.staff@example.com",
    "phone": "9800000001",
    "role": "staff"
  }
}
```

### GET `/api/admin/staff-list`

- Auth: None in current implementation
- Success response:

```json
[
  {
    "id": 3,
    "name": "Sita Staff",
    "email": "sita.staff@example.com",
    "phone": "9800000001",
    "role": "staff"
  }
]
```

- Frontend note:
  - Use this to populate staff dropdowns for appointment booking and invoice creation.

### PUT `/api/admin/update-role/{id}`

- Auth: None in current implementation
- Route params:
  - `id`: user ID
- Body:

```json
{
  "role": "staff"
}
```

- Success response:

```json
{
  "message": "User role updated successfully.",
  "user": {
    "id": 15,
    "name": "Nabin Karki",
    "email": "nabin@example.com",
    "phone": "9811111111",
    "role": "staff"
  }
}
```

- Failure response:

```json
"Invalid role. Allowed roles are admin, staff, or customer."
```

### DELETE `/api/admin/delete-staff/{id}`

- Auth: None in current implementation
- Route params:
  - `id`: user ID
- Success response:

```json
{
  "message": "Staff deleted successfully."
}
```

## InvoicesController

Base route: `api/invoices`

### GET `/api/invoices/history`

- Auth: Bearer token with role `Customer`
- Purpose: Get current customer's invoice history
- Success response:

```json
[
  {
    "id": 21,
    "customerId": 12,
    "vendorId": 2,
    "staffId": 3,
    "type": "Service",
    "totalAmount": 1800.0,
    "discountApplied": 0.0,
    "paymentStatus": "Pending",
    "invoiceDate": "2026-05-19T09:15:00Z",
    "dueDate": "2026-05-30T00:00:00Z",
    "customer": null,
    "vendor": null,
    "staff": null,
    "invoiceItems": [
      {
        "id": 31,
        "invoiceId": 21,
        "partId": 1,
        "quantity": 2,
        "invoice": null,
        "part": null
      }
    ]
  }
]
```

### POST `/api/invoices?customerId={customerId}`

- Auth: Bearer token with role `Staff` or `Admin`
- Query params:
  - `customerId`: required
- Body:

```json
{
  "vendorId": 2,
  "staffId": 3,
  "type": "Service",
  "dueDate": "2026-05-30T00:00:00Z",
  "items": [
    {
      "partId": 1,
      "quantity": 2,
      "unitPrice": 450.0
    },
    {
      "partId": 8,
      "quantity": 1,
      "unitPrice": 900.0
    }
  ]
}
```

- How total is calculated:
  - `sum(quantity * unitPrice)` from the request
  - if total is greater than `5000`, a 10% discount is applied
  - `paymentStatus` is forced to `Pending`
  - `invoiceDate` is forced to `DateTime.UtcNow`

- Success response:

```json
{
  "id": 21,
  "customerId": 15,
  "vendorId": 2,
  "staffId": 3,
  "type": "Service",
  "totalAmount": 1800.0,
  "discountApplied": 0.0,
  "paymentStatus": "Pending",
  "invoiceDate": "2026-05-19T09:15:00Z",
  "dueDate": "2026-05-30T00:00:00Z",
  "customer": null,
  "vendor": null,
  "staff": null,
  "invoiceItems": [
    {
      "id": 31,
      "invoiceId": 21,
      "partId": 1,
      "quantity": 2,
      "invoice": null,
      "part": null
    },
    {
      "id": 32,
      "invoiceId": 21,
      "partId": 8,
      "quantity": 1,
      "invoice": null,
      "part": null
    }
  ]
}
```

- Frontend note:
  - Get `customerId` from `GET /api/staff/customers/search`.
  - Get `vendorId` from `GET /api/vendor/list`.
  - Get `partId` values from `GET /api/parts`.
  - If staff is selected manually, get `staffId` from `GET /api/admin/staff-list`.

## InvoiceController

Base route: `api/invoice`

This controller is separate from `api/invoices`.

### POST `/api/invoice/purchase`

- Auth: None in current implementation
- Purpose: Create a purchase invoice via `PurchaseInvoiceService`
- Body:

```json
{
  "customerId": 0,
  "vendorId": 2,
  "staffId": 3,
  "type": "AnythingSentHereWillBeOverwritten",
  "totalAmount": 2500.0,
  "discountApplied": 0.0,
  "paymentStatus": "Pending",
  "invoiceDate": "2026-05-19T00:00:00Z",
  "dueDate": "2026-05-30T00:00:00Z",
  "invoiceItems": [
    {
      "partId": 1,
      "quantity": 10
    }
  ]
}
```

- Service behavior:
  - `type` is forced to `Purchase`
  - `invoiceDate` is forced to current UTC time
  - `customerId` is overwritten with `staffId`
  - each part stock quantity is increased by the invoice item quantity

- Success response:

```json
{
  "id": 41,
  "customerId": 3,
  "vendorId": 2,
  "staffId": 3,
  "type": "Purchase",
  "totalAmount": 2500.0,
  "discountApplied": 0.0,
  "paymentStatus": "Pending",
  "invoiceDate": "2026-05-19T10:00:00Z",
  "dueDate": "2026-05-30T00:00:00Z",
  "customer": null,
  "vendor": null,
  "staff": null,
  "invoiceItems": [
    {
      "id": 51,
      "invoiceId": 41,
      "partId": 1,
      "quantity": 10,
      "invoice": null,
      "part": null
    }
  ]
}
```

### POST `/api/invoice/sales`

- Auth: None in current implementation
- Purpose: Create a sales invoice via `SalesInvoiceService`
- Body:

```json
{
  "customerId": 15,
  "vendorId": 0,
  "staffId": 3,
  "type": "AnythingSentHereWillBeOverwritten",
  "totalAmount": 6000.0,
  "discountApplied": 0.0,
  "paymentStatus": "Pending",
  "invoiceDate": "2026-05-19T00:00:00Z",
  "dueDate": "2026-05-30T00:00:00Z",
  "invoiceItems": [
    {
      "partId": 1,
      "quantity": 2
    }
  ]
}
```

- Service behavior:
  - if `vendorId` is `0`, backend changes it to `1`
  - `type` is forced to `Sales`
  - `invoiceDate` is forced to current UTC time
  - if `totalAmount > 5000`, a 10% discount is applied
  - each part stock quantity is reduced by the invoice item quantity

- Success response:

```json
{
  "id": 42,
  "customerId": 15,
  "vendorId": 1,
  "staffId": 3,
  "type": "Sales",
  "totalAmount": 5400.0,
  "discountApplied": 600.0,
  "paymentStatus": "Pending",
  "invoiceDate": "2026-05-19T10:05:00Z",
  "dueDate": "2026-05-30T00:00:00Z",
  "customer": null,
  "vendor": null,
  "staff": null,
  "invoiceItems": [
    {
      "id": 52,
      "invoiceId": 42,
      "partId": 1,
      "quantity": 2,
      "invoice": null,
      "part": null
    }
  ]
}
```

### GET `/api/invoice`

- Auth: None in current implementation
- Purpose: Fetch all invoices ordered by latest first
- Success response:

```json
[
  {
    "id": 42,
    "customerId": 15,
    "vendorId": 1,
    "staffId": 3,
    "type": "Sales",
    "totalAmount": 5400.0,
    "discountApplied": 600.0,
    "paymentStatus": "Pending",
    "invoiceDate": "2026-05-19T10:05:00Z",
    "dueDate": "2026-05-30T00:00:00Z",
    "customer": null,
    "vendor": null,
    "staff": null,
    "invoiceItems": [
      {
        "id": 52,
        "invoiceId": 42,
        "partId": 1,
        "quantity": 2,
        "invoice": null,
        "part": null
      }
    ]
  }
]
```

## WeatherForecastController

Base route: `weatherforecast`

This is the default sample controller created by the ASP.NET template.

### GET `/weatherforecast`

- Auth: None
- Success response:

```json
[
  {
    "date": "2026-05-20",
    "temperatureC": 24,
    "temperatureF": 75,
    "summary": "Warm"
  }
]
```

## Quick route checklist

All controller routes currently present in `Controllers/`:

- `POST /api/auth/register`
- `POST /api/auth/admin/register-user`
- `POST /api/auth/secret-admin-register`
- `GET /api/auth/verify-email`
- `POST /api/auth/login`
- `GET /api/profile`
- `PUT /api/profile`
- `GET /api/vehicles`
- `POST /api/vehicles`
- `PUT /api/vehicles/{id}`
- `DELETE /api/vehicles/{id}`
- `POST /api/appointments`
- `GET /api/appointments/my-appointments`
- `POST /api/partrequests`
- `POST /api/reviews`
- `POST /api/vendor/create`
- `GET /api/vendor/list`
- `GET /api/vendor/{id}`
- `PUT /api/vendor/update/{id}`
- `DELETE /api/vendor/delete/{id}`
- `GET /api/parts`
- `GET /api/parts/{id}`
- `POST /api/parts`
- `PUT /api/parts/{id}`
- `DELETE /api/parts/{id}`
- `POST /api/staff/customers`
- `GET /api/staff/customers/{customerId}`
- `GET /api/staff/customers/search`
- `POST /api/admin/create-staff`
- `GET /api/admin/staff-list`
- `PUT /api/admin/update-role/{id}`
- `DELETE /api/admin/delete-staff/{id}`
- `GET /api/invoices/history`
- `POST /api/invoices`
- `POST /api/invoice/purchase`
- `POST /api/invoice/sales`
- `GET /api/invoice`
- `GET /weatherforecast`

## Updated APIs

### Admin role-based user list

- `GET /api/admin/users?role={role}`
- Auth: Bearer token with role `Admin`
- Purpose: fetch users by role (`admin`, `staff`, or `customer`)

Example response:

```json
[
  {
    "id": 3,
    "name": "Sita Staff",
    "email": "sita.staff@example.com",
    "phone": "9800000001",
    "role": "staff"
  }
]
```

### Part requests (admin view)

- `GET /api/partrequests`
- Auth: Bearer token with role `Admin`
- Purpose: view all part requests with customer details

Example response:

```json
[
  {
    "id": 4,
    "partName": "Brake Pad Set",
    "status": "Requested",
    "customerId": 12,
    "customerName": "Aayush Sharma",
    "customerEmail": "aayush@example.com"
  }
]
```

### Invoices

- `POST /api/invoices` no longer requires `staffId` in the body.
- Sales invoice emails are sent only when an admin triggers them.

#### Send sales invoice email (admin-triggered)

- `POST /api/invoices/{id}/send-email`
- Auth: Bearer token with role `Admin`
- Purpose: send the sales invoice email to the customer on demand

Example response:

```json
{
  "message": "Invoice email sent successfully."
}
```

### Parts

Previous:
- `Part` payloads included `requestedBy`, `requestStatus`, and `requestDescription`, and were linked to users.

Now:
- Parts are no longer linked to users. Fields like `requestedBy`, `requestStatus`, and `requestDescription` have been removed from part payloads.
- `vendorId` is still required on parts.

New part response shape:

```json
{
  "id": 1,
  "vendorId": 2,
  "name": "Oil Filter",
  "unitPrice": 450.0,
  "stockQuantity": 40
}
```

### Part requests

Previous:
- Part requests were mixed with parts metadata and vendor fields in payloads.

Now:
- Part requests are linked to users (customers) only.
- No `vendorId` is required for part requests.

Create request body:

```json
{
  "partName": "Brake Pad Set"
}
```

Admin view response sample:

```json
{
  "id": 4,
  "partName": "Brake Pad Set",
  "status": "Requested",
  "customerId": 12,
  "customerName": "Aayush Sharma",
  "customerEmail": "aayush@example.com"
}
```

### Admin customer appointments view

- `GET /api/appointments/customer/`
- Auth: Bearer token with role `Admin`
- Purpose: view all appointments for all customers

## Added on 2026-05-21

### Admin Financial Reports

- `GET /api/reports/financial/daily`
- Auth: None (in current implementation)
- Purpose: Generate daily financial report

Example response:
```json
[
  {
    "date": "2026-05-20T00:00:00Z",
    "totalSales": 5400.0,
    "totalPurchases": 2500.0,
    "netProfit": 2900.0
  }
]
```

- `GET /api/reports/financial/monthly`
- Auth: None (in current implementation)
- Purpose: Generate monthly financial report

Example response:
```json
[
  {
    "year": 2026,
    "month": 5,
    "totalSales": 5400.0,
    "totalPurchases": 2500.0,
    "netProfit": 2900.0
  }
]
```

- `GET /api/reports/financial/yearly`
- Auth: None (in current implementation)
- Purpose: Generate yearly financial report

Example response:
```json
[
  {
    "year": 2026,
    "totalSales": 5400.0,
    "totalPurchases": 2500.0,
    "netProfit": 2900.0
  }
]
```

### Staff Customer Reports

- `GET /api/reports/customers/regulars`
- Auth: None (in current implementation)
- Purpose: Generate report of regular customers (more than 3 invoices)

Example response:
```json
[
  {
    "id": 12,
    "name": "Aayush Sharma",
    "email": "aayush@example.com",
    "phoneNumber": "9812345678",
    "invoiceCount": 4
  }
]
```

- `GET /api/reports/customers/high-spenders`
- Auth: None (in current implementation)
- Purpose: Generate report of highest spending customers

Example response:
```json
[
  {
    "id": 15,
    "name": "Nabin Karki",
    "email": "nabin@example.com",
    "phoneNumber": "9811111111",
    "totalSpent": 5400.0
  }
]
```

- `GET /api/reports/customers/pending-credits`
- Auth: None (in current implementation)
- Purpose: Generate report of customers with pending credits (unpaid balances)

Example response:
```json
[
  {
    "id": 15,
    "name": "Nabin Karki",
    "email": "nabin@example.com",
    "phoneNumber": "9811111111",
    "pendingAmount": 5400.0
  }
]
```

### System Background Notifications
The system includes an automatic background service (`NotificationBackgroundService`) that runs once a day to:
- Detect parts with `StockQuantity < 10` and email the admin about low stock.
- Detect unpaid invoices where `PaymentStatus` is "Pending" or "Outstanding" and `InvoiceDate` is more than a month old, and send automated email reminders to those customers.
