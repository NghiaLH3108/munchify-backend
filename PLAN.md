# PLAN: Triển Khai ToastMessage Khi Bật/Tắt Trạng Thái (Thực Đơn, Sản Phẩm & Khuyến Mãi)

## 1. Mục tiêu
Bổ sung cơ chế bật/tắt trạng thái (Active/Inactive) và hiển thị thông báo `AppToast` đồng bộ, tức thời khi quản trị viên thay đổi trạng thái bán hàng tại:
1. **Màn hình "Thực đơn & Sản phẩm" (`ProductManagementScreen`):**
   - Hiện tại: Có Switch nhưng khi bật/tắt không hiện thông báo `AppToast`. Khi xảy ra lỗi API, màn hình bị văng về thông báo lỗi full-screen dạng `Center(child: Text('Error: ...'))` làm mất bảng sản phẩm.
   - Giải pháp: Tích hợp `BlocConsumer`, phát `AppToast.success(...)` khi thành công và `AppToast.error(...)` khi thất bại (đồng thời hoàn tác trạng thái Switch trên UI một cách mượt mà mà không sập bảng dữ liệu).
2. **Màn hình "Combo sản phẩm" (`ManageCombosScreen`):**
   - Hiện tại: Chỉ có huy hiệu trạng thái tĩnh ("Đang bán" / "Ngừng bán"), quản trị viên muốn bật/tắt phải mở form Sửa.
   - Giải pháp: Thêm Switch thao tác bật/tắt nhanh trực tiếp trên từng dòng bảng combo. Tích hợp endpoint `PATCH /api/combos/{id}/status` ở Backend với ràng buộc logic (không cho bật combo nếu có món thành phần đang ngừng bán), và hiển thị `AppToast.success(...)` / `AppToast.error(...)` tức thì.
3. **Màn hình "Khuyến mãi" (`PromotionManagementScreen`):**
   - Hiện tại: Switch bật/tắt khuyến mãi trong bảng khi hoàn thành thì reload dữ liệu một cách im lặng, không có bất kỳ thông báo `AppToast` nào.
   - Giải pháp: Bổ sung `successMessage` vào `PromotionsLoaded` và `LoadPromotions`. Bắt sự kiện trong `BlocConsumer` của `PromotionManagementScreen` để phát `AppToast.success('Đã cập nhật trạng thái chương trình khuyến mãi.')` ngay khi gạt Switch.

---

## 2. JSON API Contract (Backend ⇄ Frontend)

### Endpoint: `PATCH /api/combos/{id}/status`
- **Quyền hạn:** `Authorize(Roles = AppRoles.Admin)`
- **Header:** `Authorization: Bearer <token>`
- **Response Thành Công (200 OK):**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Cập nhật trạng thái combo thành công.",
  "data": true
}
```
- **Response Lỗi Ràng Buộc Nghiệp Vụ (400 Bad Request):**
```json
{
  "success": false,
  "statusCode": 400,
  "message": "Không thể bật combo vì có món thành phần đang ngừng bán.",
  "data": false
}
```

---

## 3. Danh Sách Trắng Tệp Tin (File Whitelist)
Đã khóa tại `specs/munchify_checkpoint.json`:
- **Backend:**
  - `Munchify.Backend/Munchify.BLL/Service/Interfaces/IComboService.cs`
  - `Munchify.Backend/Munchify.BLL/Service/ComboService.cs`
  - `Munchify.Backend/Munchify.API/Controllers/CombosController.cs`
- **Frontend:**
  - `munchify-frontend/munchify_frontend/lib/features/products/presentation/bloc/product_admin_state.dart`
  - `munchify-frontend/munchify_frontend/lib/features/products/presentation/bloc/product_admin_bloc.dart`
  - `munchify-frontend/munchify_frontend/lib/features/products/presentation/screens/product_management_screen.dart`
  - `munchify-frontend/munchify_frontend/lib/features/combos/domain/repositories/combo_repository.dart`
  - `munchify-frontend/munchify_frontend/lib/features/combos/data/repositories/combo_repository_impl.dart`
  - `munchify-frontend/munchify_frontend/lib/features/combos/data/datasources/combo_remote_data_source.dart`
  - `munchify-frontend/munchify_frontend/lib/features/combos/domain/usecases/manage_combo_usecases.dart`
  - `munchify-frontend/munchify_frontend/lib/features/combos/presentation/bloc/combo_event.dart`
  - `munchify-frontend/munchify_frontend/lib/features/combos/presentation/bloc/combo_bloc.dart`
  - `munchify-frontend/munchify_frontend/lib/features/combos/presentation/screens/manage_combos_screen.dart`
  - `munchify-frontend/munchify_frontend/lib/features/combos/presentation/widgets/combo_form_dialog.dart`
  - `munchify-frontend/munchify_frontend/lib/features/promotions/presentation/bloc/promotion_admin_event.dart`
  - `munchify-frontend/munchify_frontend/lib/features/promotions/presentation/bloc/promotion_admin_state.dart`
  - `munchify-frontend/munchify_frontend/lib/features/promotions/presentation/bloc/promotion_admin_bloc.dart`
  - `munchify-frontend/munchify_frontend/lib/features/promotions/presentation/pages/promotion_management_screen.dart`
  - `munchify-frontend/munchify_frontend/lib/injection_container.dart`
  - `munchify-frontend/munchify_frontend/test/features/products/presentation/bloc/product_status_toggle_test.dart`
  - `munchify-frontend/munchify_frontend/test/features/promotions/presentation/bloc/promotion_status_toggle_test.dart`

---

## 4. Kế hoạch Triển khai 3 Lượt (3-Turn Protocol)

### Lượt 1: Khoanh vùng & Hợp đồng (Orchestrator) - [HIỆN TẠI]
- [x] Rà soát toàn bộ luồng bật/tắt trạng thái của Sản phẩm, Combo và Khuyến mãi.
- [x] Lập `PLAN.md` và khóa Whitelist vào `specs/munchify_checkpoint.json`.
- [ ] Dừng lại chờ Quản đốc (User) phản hồi: **"Approve Plan"**.

### Lượt 2: Thực thi Song song (Subagents)
- [ ] **Tác tử `dotnet-architect`:**
  - Cập nhật `IComboService.cs`, `ComboService.cs`, và `CombosController.cs` để thêm `PATCH /api/combos/{id}/status`.
  - Kiểm tra ràng buộc món thành phần khi bật combo.
  - Chạy `dotnet build` đạt **0 Error**.
- [ ] **Tác tử `flutter-artisan`:**
  - **Sản phẩm:** Cập nhật `ProductAdminBloc` và `ProductManagementScreen` với `BlocConsumer` phát `AppToast.success(...)` / `AppToast.error(...)`.
  - **Combo:** Bổ sung `ToggleComboStatusUseCase` và sự kiện `ToggleComboStatusEvent` vào `ComboBloc`. Thêm Switch bật/tắt nhanh và gọi `AppToast` trong `ManageCombosScreen`.
  - **Khuyến mãi:** Bổ sung `successMessage` vào `PromotionsLoaded` và `LoadPromotions`. Bắt sự kiện trong `BlocConsumer` của `PromotionManagementScreen` để phát `AppToast.success(...)`.
  - Chạy `dart analyze` đạt **0 Error**.

### Lượt 3: Thanh tra Độc lập (Inquisitor QA)
- [ ] **Tác tử `inquisitor-qa`:**
  - Kiểm tra phạm vi chỉnh sửa so với Whitelist.
  - Viết và chạy bộ test `product_status_toggle_test.dart` và `promotion_status_toggle_test.dart`.
  - Xuất báo cáo `specs/AUDIT_REPORT.md`.
- [ ] **Tác tử `git-quartermaster`:**
  - Tạo commit chuẩn hóa: `feat: HuyCG-status-toggle-toast-message-#66-UC00`.
