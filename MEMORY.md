# Munchify Project Memory

## Architectural Decisions & Lessons Learned

### Backend (.NET Core Clean Architecture)
1. **No EntityFramework in BLL (Service Layer)**:
   - *Rule*: The Microsoft.EntityFrameworkCore namespace MUST NOT be used in the Service layer (BLL).
   - *Context*: We encountered a leaky abstraction where IQueryable and .Include() were being used inside InventoryService.cs. This violates the strict boundary between the Data Access Layer (DAL) and the Business Logic Layer (BLL).
   - *Fix*: All eager loading (.Include), filtering (.Where), and querying logic MUST be encapsulated inside the Repository (Munchify.DAL/Repository). The Service should only call methods like GetProductsWithDetailsAsync() and expect a materialized collection (IEnumerable or PagedResult), not an IQueryable.

### Frontend (Flutter Clean Architecture)
1. **DataTable Layout Issue in Admin Shell**:
   - *Rule*: When rendering DataTable inside a SingleChildScrollView(scrollDirection: Axis.horizontal), the table will collapse to its minimum required width (e.g., 800px) leaving empty whitespace on large screens.
   - *Fix*: Wrap the SingleChildScrollView inside a LayoutBuilder. Use a SizedBox around the DataTable with width: constraints.maxWidth > 800 ? constraints.maxWidth : 800.0. This forces the table columns to expand proportionally and fill the entire Card, matching the design system of the Admin panels.

2. **Model/Entity Property Naming matching Backend**:
   - *Rule*: When creating Model classes for json_serializable, the property names in Dart MUST perfectly match the serialized JSON keys returned by the backend DTOs.
   - *Context*: A mismatch (e.g., Dart id vs Backend promotionId) will result in null parsing, leading to fatal TypeError crashes during UI rendering. Always verify the backend DTO structure before generating frontend Models.

3. **Flutter UI Refresh Limitations**:
   - *Rule*: Changing field names, types, or adding/removing fields in const classes (like Entities) will cause Hot Reload rejected: Exception: Const class cannot remove fields.
   - *Fix*: You must perform a **Hot Restart** (or completely stop and re-run the app) to apply structural changes to const classes.

4. **Master-Detail Data Integrity in Edit Dialogs**:
   - *Rule*: Never populate a full Edit Form directly from a summary list entity (which omits deep child collections like `products`). Always invoke the detail usecase/API (`GetPromotionById`) before opening the dialog to avoid overwriting or clearing existing relations.
   - *Rule*: Payload keys must strictly align with Backend DTOs (`productIds: List<int>`).

5. **Server-Side Validation Guards**:
   - *Rule*: Never rely solely on client-side form validation. Check uniqueness via repository (`ExistsCodeAsync`) to avoid 500 SQL Server constraint crashes, and validate logical bounds (`0 < DiscountValue <= 100` for percentage) directly in the BLL service.

