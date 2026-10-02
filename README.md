# Munchify POS - Backend API

Hệ thống Backend cho nền tảng quản lý điểm bán hàng F&B **Munchify POS**, được xây dựng trên nền tảng **.NET 8** theo kiến trúc đa tầng phân lớp rõ ràng (Layered / Clean Architecture) đảm bảo tính mở rộng, hiệu năng cao và bảo mật nghiêm ngặt.

---

## 📌 Mục lục
- [Kiến trúc hệ thống](#-kiến-trúc-hệ-thống)
- [Công nghệ sử dụng](#-công-nghệ-sử-dụng)
- [Tính năng Quản lý Nhân sự (Staff Management)](#-tính-năng-quản-lý-nhân-sự-staff-management)
- [Chi tiết API Endpoints](#-chi-tiết-api-endpoints)
- [Bảo mật & Tính toàn vẹn dữ liệu](#-bảo-mật--tính-toàn-vẹn-dữ-liệu)
- [Quy chuẩn Git & Phân nhánh (Branching Strategy)](#-quy-chuẩn-git--phân-nhánh-branching-strategy)
- [Hướng dẫn cài đặt & Khởi chạy](#-hướng-dẫn-cài-đặt--khởi-chạy)

---

## 🏗 Kiến trúc hệ thống

Dự án được tổ chức theo mô hình Layered Architecture phân tách độc lập trách nhiệm giữa các tầng:

```
Munchify.Backend/
├── Munchify.API/             # Presentation Layer: Controllers, CORS, DI, Swagger, Middleware
│   ├── Controllers/          # UsersController, v.v.
│   ├── Program.cs            # Khởi tạo App, Service Registration & Middleware Pipeline
│   └── appsettings.json      # Cấu hình chuỗi kết nối Database & môi trường
├── Munchify.BLL/             # Business Logic Layer: Nghiệp vụ cốt lõi, Validation, Hash mật khẩu
│   ├── Service/              # UserService, v.v.
│   └── Interfaces/           # IUserService
└── Munchify.DAL/             # Data Access Layer: EF Core, Database, Repositories, Entities, DTOs
    ├── Entities/             # User, Role, v.v.
    ├── DTOs/                 # CreateUserRequest, UpdateUserRequest, UserDto, ApiResponse
    ├── Context/              # MunchifyDbContext
    └── Repository/           # UserRepository, IUserRepository
```

---

## ⚡ Công nghệ sử dụng

- **Framework**: .NET 8 (ASP.NET Core Web API)
- **ORM**: Entity Framework Core 8.x
- **Database**: Microsoft SQL Server / LocalDB
- **Bảo mật**: BCrypt.Net-Next (Mã hóa mật khẩu bằng thuật toán Salted Hash chuẩn công nghiệp)
- **Tài liệu API**: OpenAPI / Swagger UI
- **Cross-Origin**: CORS Policy mở cho môi trường phát triển cục bộ và Flutter Web

---

## 👥 Tính năng Quản lý Nhân sự (Staff Management)

Module Quản lý Nhân sự hỗ trợ đầy đủ các nghiệp vụ quản trị tài khoản điểm bán:

1. **Xem danh sách nhân sự (View Staff List)**: Hỗ trợ phân trang (`page`, `pageSize`), tìm kiếm theo từ khóa họ tên / username, lọc theo vai trò (`STAFF`, `ADMIN`) và lọc trạng thái hoạt động (`isActive`).
2. **Xem chi tiết nhân sự (View Staff Detail)**: Tra cứu thông tin chi tiết của nhân viên theo ID.
3. **Tạo tài khoản nhân viên (Create Staff Account)**: 
   - Mặc định phân quyền tài khoản là `STAFF` (Nhân viên điểm bán / Thu ngân).
   - Kiểm tra trùng lặp `username`, `phone`, `email`.
   - Tự động mã hóa mật khẩu bằng BCrypt trước khi lưu trữ vào cơ sở dữ liệu.
4. **Cập nhật thông tin nhân viên (Update Staff Information)**: Cho phép chỉnh sửa họ tên, số điện thoại, email với cơ chế kiểm tra không trùng lặp với tài khoản khác.
5. **Khóa / Mở khóa tài khoản (Lock/Unlock Staff Account)**: 
   - Đảo trạng thái kích hoạt `isActive` của tài khoản tức thì.
   - **Cơ chế bảo vệ đặc biệt**: Không cho phép khóa tài khoản `ADMIN` duy nhất đang hoạt động để tránh rủi ro mất quyền quản trị hệ thống.

---

## 📡 Chi tiết API Endpoints

Base URL: `http://localhost:5194`

| Phương thức | Endpoint | Mô tả | Tham số / Body |
| :--- | :--- | :--- | :--- |
| `GET` | `/api/users` | Lấy danh sách nhân viên phân trang & lọc | Query: `role`, `isActive`, `page`, `pageSize` |
| `GET` | `/api/users/{id}` | Lấy chi tiết nhân viên theo ID | Route: `id` (int) |
| `POST` | `/api/users/staff` | Tạo tài khoản nhân viên mới | Body: `CreateUserRequest` (`username`, `password`, `fullName`, `phone`, `email`, `role: "STAFF"`) |
| `PUT` | `/api/users/{id}` | Cập nhật thông tin nhân viên | Route: `id`, Body: `UpdateUserRequest` (`fullName`, `phone`, `email`) |
| `PATCH` | `/api/users/{id}/status` | Khóa / Mở khóa tài khoản | Route: `id` (int) |

### Ví dụ Request tạo tài khoản:
```json
POST /api/users/staff
Content-Type: application/json

{
  "username": "staff_linh",
  "password": "Password@123",
  "fullName": "Nguyễn Khánh Linh",
  "phone": "0987654321",
  "email": "linhnk@munchify.pos",
  "role": "STAFF"
}
```

### Định dạng phản hồi chuẩn (`ApiResponse<T>`):
```json
{
  "success": true,
  "message": "Tạo tài khoản nhân viên thành công",
  "data": {
    "userId": 5,
    "username": "staff_linh",
    "fullName": "Nguyễn Khánh Linh",
    "phone": "0987654321",
    "email": "linhnk@munchify.pos",
    "role": "STAFF",
    "isActive": true,
    "createdAt": "2026-09-22T08:00:00Z"
  },
  "errors": []
}
```

---

## 🛡 Bảo mật & Tính toàn vẹn dữ liệu

- **Không lưu mật khẩu dạng plain-text**: 100% mật khẩu được hash thông qua `BCrypt.Net.BCrypt.HashPassword()`.
- **Chống SQL Injection**: Sử dụng Parameterized Queries và EF Core LINQ abstraction.
- **Ràng buộc duy nhất (Unique Constraints)**: Ngăn chặn triệt để tình trạng trùng lặp Username, Email và Số điện thoại giữa các nhân sự.
- **Chính sách phân quyền an toàn**: Tạo tài khoản mới từ giao diện quản trị nhân viên luôn mặc định gán quyền `STAFF`.

---

## 🌿 Quy chuẩn Git & Phân nhánh (Branching Strategy)

> [!IMPORTANT]
> **Tuyệt đối không push trực tiếp lên nhánh `main`**. Nhánh `main` chỉ dành cho các bản phát hành Production ổn định.

1. **Nhánh gốc phát triển**: `dev`
2. **Nhánh tính năng (Feature Branch)**: `manage-staff-HuyCG` (hoặc định dạng `<feature-name>-<DeveloperName>`)
3. **Quy trình làm việc**:
   - Nhánh tính năng được tạo từ `dev`: `git checkout -b manage-staff-HuyCG dev`
   - Commit code theo quy chuẩn **Conventional Commits**:
     - `feat: ...` : Thêm mới tính năng hoặc endpoint
     - `fix: ...` : Sửa lỗi
     - `docs: ...` : Cập nhật tài liệu
     - `test: ...` : Viết test hoặc kiểm thử
     - `chore: ...` : Cấu hình hoặc bảo trì dependencies
   - Tạo Pull Request hướng vào nhánh `dev` sau khi hoàn thành và được kiểm thử đầy đủ.

---

## 🚀 Hướng dẫn cài đặt & Khởi chạy

### 1. Yêu cầu hệ thống
- .NET 8.0 SDK trở lên
- SQL Server (LocalDB, Express hoặc Developer Edition)

### 2. Cấu hình chuỗi kết nối
Mở file `Munchify.Backend/Munchify.API/appsettings.json` và kiểm tra chuỗi kết nối `DefaultConnection`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=FnBStoreDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 3. Chạy ứng dụng
```bash
# Khôi phục packages
dotnet restore

# Build dự án
dotnet build

# Chạy Backend API
dotnet run --project Munchify.Backend/Munchify.API
```

API sẽ khởi động tại:
- HTTP: `http://localhost:5194`
- Swagger UI tài liệu tương tác: `http://localhost:5194/swagger`
