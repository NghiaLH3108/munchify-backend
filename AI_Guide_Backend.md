# AI Agent & Developer Guidelines: Munchify Backend API

## 1. Document Purpose & Operational Mandate

This document serves as the single source of truth for **AI Coding Agents** (Claude, Cursor, Copilot, Gemini) and software engineers working on the **Munchify Backend API**.
Every generated class, method, endpoint, and test must strictly follow the architectural boundaries, naming conventions, business constraints, and database rules detailed below.

---

## 2. Tech Stack & Environment

- **Language & Runtime:** C# / .NET 8+ (or latest LTS)
- **Framework:** ASP.NET Core Web API
- **Architecture Pattern:** N-Layered Architecture (Presentation -> Business Logic / Service -> Repository / Data Access -> Database)
- **Target IDE:** Visual Studio 2022+ / Visual Studio Code
- **ORM:** Entity Framework Core 8+ (Code-First with DB synchronization)
- **Database Engine:** Microsoft SQL Server (FnBStoreDB)
- **Authentication:** JWT (JSON Web Token) with Claims-based Authorization
- **Documentation & API Testing:** Swagger / OpenAPI

---

## 3. Layered Architecture & Solution Structure

To preserve strict separation of concerns, dependencies MUST flow in one direction:
`Controllers (Presentation)` -> `Services (Business Logic)` -> `Repositories (Data Access)` -> `DbContext & Database`.
Controllers NEVER access `DbContext` or `Repositories` directly.

```
Munchify.Backend/
│
├── Munchify.API/                         # [Layer 1: Presentation Layer]
│   ├── Controllers/                      # REST API Endpoints
│   │   ├── AuthController.cs
│   │   ├── CategoriesController.cs
│   │   ├── UnitsController.cs
│   │   ├── ProductsController.cs
│   │   ├── CustomersController.cs
│   │   ├── OrdersController.cs
│   │   ├── PaymentsController.cs
│   │   ├── SuppliersController.cs
│   │   ├── StockImportsController.cs
│   │   ├── PromotionsController.cs
│   │   └── ReportsController.cs
│   ├── Middlewares/                      # Global Exception Handling, JWT Auth Middlewares
│   │   └── ExceptionMiddleware.cs
│   ├── Filters/                          # Action & Validation Filters
│   ├── appsettings.json                  # Connection strings & JWT configurations
│   └── Program.cs                        # Dependency Injection, Middleware Pipeline
│
├── Munchify.BLL/                         # [Layer 2: Business Logic Layer (BLL)]
│   ├── Service/
│   │   ├── Interfaces/
│   │   │   ├── IAuthService.cs
│   │   │   ├── ICategoryService.cs
│   │   │   ├── IUnitService.cs
│   │   │   ├── IProductService.cs
│   │   │   ├── ICustomerService.cs
│   │   │   ├── IOrderService.cs             # Core POS checkout, calculations & transactions
│   │   │   ├── IStockImportService.cs       # Warehouse replenishment logic
│   │   │   ├── IInventoryService.cs         # StockMovements tracking & deduction
│   │   │   ├── IPromotionService.cs
│   │   │   └── IReportService.cs
│   │   ├── AuthService.cs
│   │   ├── CategoryService.cs
│   │   ├── UnitService.cs
│   │   ├── ProductService.cs
│   │   ├── CustomerService.cs
│   │   ├── OrderService.cs
│   │   ├── StockImportService.cs
│   │   ├── InventoryService.cs
│   │   ├── PromotionService.cs
│   │   └── ReportService.cs
│   └── Helpers/ / Mappers/              # AutoMapper / Mapping Extensions
│
└── Munchify.DAL/                 # [Layer 3: Data Access Layer (DAL)]
    ├── Repository/
    │   ├── Interfaces/
    │   │   ├── IGenericRepository.cs
    │   │   ├── IUnitOfWork.cs               # Transaction & commit management
    │   │   ├── IUserRepository.cs
    │   │   ├── IProductRepository.cs
    │   │   ├── IOrderRepository.cs
    │   │   ├── ICustomerRepository.cs
    │   │   ├── IStockImportRepository.cs
    │   │   ├── IStockMovementRepository.cs
    │   │   └── IPromotionRepository.cs
    │   ├── GenericRepository.cs
    │   ├── UnitOfWork.cs
    │   ├── UserRepository.cs
    │   ├── ProductRepository.cs
    │   ├── OrderRepository.cs
    │   ├── CustomerRepository.cs
    │   ├── StockImportRepository.cs
    │   ├── StockMovementRepository.cs
    │   └── PromotionRepository.cs
    │
    ├── Data/                           # [Database Context & Migrations]
    │   ├── AppDbContext.cs             # EF Core DbContext with Fluent API configuration
    │   └── Migrations/                 # EF Core migration history
    │
    ├── Entities/                       # Direct representation of SQL Server tables
    │   ├── User.cs
    │   ├── Category.cs
    │   ├── Unit.cs
    │   ├── Product.cs
    │   ├── ProductCombo.cs
    │   ├── Customer.cs
    │   ├── Order.cs
    │   ├── OrderDetail.cs
    │   ├── Payment.cs
    │   ├── StockMovement.cs
    │   ├── Supplier.cs
    │   ├── StockImport.cs
    │   ├── StockImportDetail.cs
    │   ├── Promotion.cs
    │   └── PromotionProduct.cs
    ├── Enums/                      # Roles, OrderStatus, MovementType, etc.
    │
    └── DTOs/                       # [Data Transfer Objects & Contracts]
        ├── Common/                          # ApiResponse<T>, PagedResult<T>
        ├── Auth/                            # LoginRequest, LoginResponse, UserDto
        ├── Products/                        # ProductRequest, ProductResponse, ComboItemDto
        ├── Orders/                          # CreateOrderRequest, OrderDetailDto, OrderResponse
        ├── Customers/                       # CustomerDto, CreateCustomerRequest
        ├── StockImports/                    # CreateImportRequest, CompleteImportResponse
        ├── Promotions/                      # PromotionDto, ApplyPromotionRequest
        └── Reports/                         # RevenueSummaryDto, TopSellingProductDto
```

---

## 4. Database Schema & Data Modeling (FnBStoreDB)

The database consists of **15 interconnected tables** configured with strict referential integrity.

### 4.1 Schema Matrix

1. **Users**: `UserId` (PK), `Username`, `PasswordHash`, `FullName`, `Phone`, `Email`, `Role` ('ADMIN', 'STAFF'), `IsActive`, `CreatedAt`, `UpdatedAt`.
2. **Categories**: `CategoryId` (PK), `CategoryName`, `Description`, `IsActive`, `CreatedAt`, `UpdatedAt`.
3. **Units**: `UnitId` (PK), `UnitName`, `Description`, `IsActive`.
4. **Products**: `ProductId` (PK), `CategoryId` (FK), `UnitId` (FK), `ProductCode`, `ProductName`, `Description`, `Price`, `StockQuantity`, `MinStockLevel`, `ImageUrl`, `IsCombo` (0: Single, 1: Combo), `IsActive`, `CreatedAt`, `UpdatedAt`.
5. **ProductCombos**: `ComboId` (PK), `ParentProductId` (FK -> Products), `ComponentProductId` (FK -> Products), `Quantity`.
6. **Customers**: `CustomerId` (PK), `CustomerName`, `Phone` (Unique), `LoyaltyPoints`, `CreatedAt`, `UpdatedAt`.
7. **Orders**: `OrderId` (PK), `OrderCode` (Unique), `StaffId` (FK -> Users), `CustomerId` (FK -> Customers, Nullable), `Subtotal`, `DiscountAmount`, `TotalAmount`, `PointsEarned`, `PointsRedeemed`, `OrderStatus` ('PENDING', 'COMPLETED', 'CANCELLED'), `PaymentStatus` ('PENDING', 'PAID', 'FAILED'), `CreatedAt`, `CompletedAt`, `CancelledAt`.
8. **OrderDetails**: `OrderDetailId` (PK), `OrderId` (FK -> Orders), `ProductId` (FK -> Products), `ProductName`, `UnitName`, `UnitPrice`, `Quantity`, `DiscountAmount`, `TotalAmount`.
9. **Payments**: `PaymentId` (PK), `OrderId` (FK -> Orders), `PaymentMethod` ('CASH', 'QR_CODE', 'BANK_TRANSFER'), `Amount`, `PaymentStatus` ('PENDING', 'PAID', 'FAILED', 'REFUNDED'), `TransactionCode`, `PaidAt`, `CreatedAt`.
10. **StockMovements**: `StockMovementId` (PK), `ProductId` (FK -> Products), `UserId` (FK -> Users), `MovementType` ('IMPORT', 'SALE', 'COMBO_SALE', 'ADJUSTMENT', 'DAMAGE'), `Quantity` (+ / -), `PreviousQuantity`, `NewQuantity`, `ReferenceType` ('ORDER', 'STOCK_IMPORT', 'MANUAL'), `ReferenceId`, `Reason`, `CreatedAt`.
11. **Suppliers**: `SupplierId` (PK), `SupplierName`, `ContactName`, `Phone`, `Address`, `IsActive`, `CreatedAt`.
12. **StockImports**: `ImportId` (PK), `ImportCode` (Unique), `SupplierId` (FK -> Suppliers), `UserId` (FK -> Users), `TotalAmount`, `Status` ('DRAFT', 'COMPLETED', 'CANCELLED'), `ImportDate`, `Notes`.
13. **StockImportDetails**: `ImportDetailId` (PK), `ImportId` (FK -> StockImports), `ProductId` (FK -> Products), `Quantity`, `ImportPrice`, `TotalAmount`.
14. **Promotions**: `PromotionId` (PK), `PromotionCode` (Unique), `PromotionName`, `Description`, `DiscountType` ('PERCENTAGE', 'FIXED_AMOUNT'), `DiscountValue`, `MinimumOrderAmount`, `StartDate`, `EndDate`, `IsActive`, `CreatedAt`, `UpdatedAt`.
15. **PromotionProducts**: `PromotionId` (FK, PK), `ProductId` (FK, PK).

---

## 5. Master Feature & Functional Specification

### Module 1: Authentication & Account Management

- **F-01.1 (Login):** Authenticate credentials (`Username`, `PasswordHash` via BCrypt). Generate signed JWT Token embedding `UserId`, `Username`, `FullName`, and `Role`.
- **F-01.2 (User Profile):** Fetch logged-in user profile (`GET /api/auth/me`).
- **F-01.3 (User Management - Admin Only):** Create, update, deactivate staff accounts (`ADMIN`, `STAFF`).

### Module 2: Catalog & Master Data Management

- **F-02.1 (Categories):** CRUD categories. Filter active ones for POS.
- **F-02.2 (Units):** CRUD measurement units (Bịch, Gói, Chai, Lon, Ly).
- **F-02.3 (Single Products):** CRUD single items with price, image, code, and minimum stock alarm thresholds.
- **F-02.4 (Combo Packages):** Create and configure composite products (`IsCombo = 1`) linked to one or more single products with quantities via `ProductCombos`.

### Module 3: Warehouse & Stock Replenishment

- **F-03.1 (Supplier Management):** CRUD supplier directory.
- **F-03.2 (Draft Stock Import):** Create purchase orders/imports with supplier and batch item details (`Status = 'DRAFT'`).
- **F-03.3 (Complete Stock Import - Admin):** Finalize stock import:
  - Transitions `StockImports.Status` to `'COMPLETED'`.
  - Increments `Products.StockQuantity` by imported quantity.
  - Writes audit log into `StockMovements` (`MovementType = 'IMPORT'`).
- **F-03.4 (Manual Stock Adjustment & Damage Reporting):** Adjust stock counts with reason (`ADJUSTMENT`, `DAMAGE`) and log audit movement.

### Module 4: Customer Loyalty Program

- **F-04.1 (Customer Lookup):** Search customer by unique phone number; fetch current `LoyaltyPoints`.
- **F-04.2 (Customer Registration):** Create new customer record at POS during checkout.
- **F-04.3 (Point Calculation & Redemption):**
  - Accrue 1 point per 100,000 VNĐ paid.
  - Redeem existing points (1 point = 1,000 VNĐ discount or configured rate).

### Module 5: Core POS Checkout & Transaction Management

- **F-05.1 (Validate Stock):** Verify availability for both single items and combo constituent components.
- **F-05.2 (Server-Side Pricing & Promotions):** Validate promotion code, check expiry date, minimum order amount, calculate discounts.
- **F-05.3 (Atomic Checkout Execution):**
  - Execute inside an explicit SQL Transaction (`using var transaction = await _dbContext.Database.BeginTransactionAsync()`).
  - Create `Orders` and `OrderDetails` (storing historical snapshot of `ProductName`, `UnitName`, `UnitPrice`).
  - Create `Payments` record (`CASH`, `QR_CODE`, `BANK_TRANSFER`).
  - Decrement inventory for single products (`MovementType = 'SALE'`).
  - Decrement inventory for combo components based on `ProductCombos.Quantity * OrderItem.Quantity` (`MovementType = 'COMBO_SALE'`).
  - Update customer loyalty points.
  - Commit transaction.

### Module 6: Promotions & Discount Rules

- **F-06.1 (Promotion Rules):** Manage promo codes, validity dates, minimum order thresholds, discount type (`PERCENTAGE` vs `FIXED_AMOUNT`).
- **F-06.2 (Promotion Products Binding):** Restrict discounts to specific products via `PromotionProducts`.

### Module 7: Reporting & Business Intelligence

- **F-07.1 (Daily / Period Revenue Summary):** Total revenue, order count, average order value, payment method breakdown.
- **F-07.2 (Top-Selling Products):** Top performing single items and combos by revenue and volume.
- **F-07.3 (Low-Stock Alerting):** Query products where `StockQuantity <= MinStockLevel`.

### Module 8: AI Demand Forecasting & Low-Stock Early Warning
* **F-08.1 (Run-Rate Calculation & Days Remaining):**
* Endpoint: `GET /api/inventory/forecasting`
* Algorithm: Analyzes sales history from the `OrderDetails` table and stock fluctuations from `StockMovements` (`MovementType` in `['SALE', 'COMBO_SALE']`) over the last 14–30 days to calculate the average daily consumption rate ($V_{avg}$). 
* Formula for estimating remaining inventory days:
$$\text{DaysRemaining} = \frac{\text{Products.StockQuantity}}{V_{avg}}$$
* **F-08.2 (Stockout Risk Alerting):**
* Automatically flags stockout risks (`CRITICAL` / `WARNING`) when:
$$\text{DaysRemaining} \le \text{LeadTime}_{\text{supplier}} \quad (\text{Default: } 3 \text{ days}) \quad \text{or} \quad \text{StockQuantity} \le \text{MinStockLevel}$$
* **F-08.3 (Smart Purchase Quantity Recommendation):**
* Calculates the recommended import quantity from suppliers based on a buffer cycle ($\text{BufferDays}$, default: 7 days):
$$\text{SuggestedImportQty} = \max\left(0, \lceil V_{avg} \times \text{BufferDays} + \text{MinStockLevel} - \text{StockQuantity} \rceil\right)$$
* **F-08.4 (One-Click Draft Import Generation):**
* Endpoint: `POST /api/stock-imports/generate-from-forecast`
* Automatically generates draft stock import records (`StockImports` with `Status = 'DRAFT'`), grouped by `SupplierId` and including a list of `StockImportDetails` based on the recommended quantities, eliminating the need for manual data entry by staff.

---

## 6. Business Rules & Server-Side Constraints

- **BR-01 (Active Items Only):** Only products and categories with `IsActive = true` can be returned to POS endpoints or ordered.
- **BR-02 (Zero Negative Stock Invariance):** Stock quantity must NEVER drop below 0.
  $$\text{StockAvailable}_{\text{component}} \ge \text{Quantity}_{\text{in\_combo}} \times \text{Quantity}_{\text{ordered}}$$
- **BR-03 (Server Calculation Authority):** Client amounts are treated as untrusted hints. Subtotal, discounts, points, and final total amount MUST be computed and validated by the backend service layer.
- **BR-04 (Price Snapshot Immutability):** `OrderDetails` must store static historical snapshots (`ProductName`, `UnitName`, `UnitPrice`, `TotalAmount`). Product updates never mutate past orders.
- **BR-05 (Combo Inventory Propagation):** Combos have no standalone physical stock (`StockQuantity = 0`). Inventory deduction always cascades down to `ProductCombos.ComponentProductId`.
- **BR-06 (Loyalty Point Formula):**
  $$\text{PointsEarned} = \left\lfloor \frac{\text{TotalAmount}}{100,000} \right\rfloor$$
- **BR-07 (Strict Traceability):** Every inventory adjustment requires an immutable entry in `StockMovements` with previous, new, and diff quantities plus reference ID.

---

## 7. Mandatory Coding Standards & Guidelines for AI Agents

1. **Explicit Asynchronous Patterns:** Always use `async`/`await` with `CancellationToken` support in repositories and services (`async Task<T>`, `SaveChangesAsync`).
2. **DTO & ViewModel Separation:**
   - NEVER expose EF Core Entity models directly in Controller action return types.
   - Map to strongly typed DTOs using explicit mapping or AutoMapper.
3. **Database Transactions:**
   - Any multi-table write operations (e.g., Order checkout, Import completion) MUST execute inside `IDbContextTransaction`.
4. **Standardized API Response Envelope:**
   ```csharp
   public class ApiResponse<T>
   {
       public bool Success { get; set; }
       public string Message { get; set; } = string.Empty;
       public T? Data { get; set; }
       public List<string>? Errors { get; set; }
   }
   ```
5. **Exception Handling:**
   - Throw custom domain exceptions (`NotFoundException`, `InsufficientStockException`, `ValidationException`).
   - Catch and transform to HTTP 400, 404, 500 status codes via global `ExceptionMiddleware`.
6. **No Business Logic in Controllers:** Controllers must remain slim; their only responsibility is binding DTOs, invoking services, and returning `ActionResult<ApiResponse<T>>`.
