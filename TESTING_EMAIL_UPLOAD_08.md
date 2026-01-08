# ?? Testing Guide: Email Upload Cam K?t TT08

## ?? Test Cases

### ? Test Case 1: T?o Contract + G?i Email T? ??ng

**Preconditions:**
- User ID 38 t?n t?i trong database
- User có email h?p l?
- SMTP config ?ã ?úng trong appsettings.json

**Steps:**
```bash
curl -X POST "http://localhost:5000/api/SalaryContracts" \
  -H "Authorization: Bearer {admin_token}" \
  -H "Content-Type: multipart/form-data" \
  -F "UserId=38" \
  -F "BaseSalary=15000000" \
  -F "InsuranceSalary=0" \
  -F "ContractType=FREELANCE" \
  -F "DependentsCount=0" \
  -F "HasCommitment08=true"
```

**Expected Results:**
- ? HTTP 201 Created
- ? Response message: "T?o h?p ??ng l??ng thành công. Email h??ng d?n upload cam k?t ?ã ???c g?i."
- ? Contract saved v?i `HasCommitment08 = true`, `AttachmentPath = null`
- ? Email ???c g?i ??n user.Email
- ? Log: "Sent commitment notification email to user 38 for contract {id}"

**Email Verification:**
- ? Subject: `[ERP] C?u hình l??ng thành công - Vui lòng upload Cam k?t Thông t? 08`
- ? From: `ERP System <noreply@erpsystem.com>`
- ? To: User's email
- ? Body ch?a:
  - Tên user
  - Thông tin l??ng (formatted: 15,000,000 VN?)
  - Link download template (clickable)
  - Link upload (clickable, ?úng frontend URL)
  - Deadline (CreatedAt + 7 ngày)

---

### ? Test Case 2: Không G?i Email Khi HasCommitment08 = false

**Steps:**
```bash
curl -X POST "http://localhost:5000/api/SalaryContracts" \
  -H "Authorization: Bearer {admin_token}" \
  -H "Content-Type: multipart/form-data" \
  -F "UserId=39" \
  -F "BaseSalary=20000000" \
  -F "InsuranceSalary=0" \
  -F "ContractType=OFFICIAL" \
  -F "DependentsCount=2" \
  -F "HasCommitment08=false"
```

**Expected Results:**
- ? HTTP 201 Created
- ? Response message: "T?o h?p ??ng l??ng thành công" (không có "Email ?ã g?i")
- ? Contract saved
- ? Không g?i email
- ? Không có log v? email

---

### ? Test Case 3: Không G?i Email Khi ?ã Có Attachment

**Steps:**
```bash
# T?o contract v?i attachment ngay
curl -X POST "http://localhost:5000/api/SalaryContracts" \
  -H "Authorization: Bearer {admin_token}" \
  -H "Content-Type: multipart/form-data" \
  -F "UserId=40" \
  -F "BaseSalary=15000000" \
  -F "InsuranceSalary=0" \
  -F "ContractType=FREELANCE" \
  -F "DependentsCount=0" \
  -F "HasCommitment08=true" \
  -F "Attachment=@test.pdf"
```

**Expected Results:**
- ? HTTP 201 Created
- ? Contract saved v?i `AttachmentPath` ?ã có giá tr?
- ? Không g?i email (vì ?ã có file)
- ? Response không ch?a "Email ?ã g?i"

---

### ? Test Case 4: Download Template

**Steps:**
```bash
curl -O "http://localhost:5000/api/SalaryContracts/download-commitment08-template"
```

**Expected Results:**
- ? HTTP 200 OK
- ? File download: `Mau_Cam_Ket_Thong_Tu_08.docx`
- ? Content-Type: `application/vnd.openxmlformats-officedocument.wordprocessingml.document`
- ? File size > 0 bytes
- ? File m? ???c b?ng Microsoft Word
- ? File ch?a form cam k?t thông t? 08

**Manual Verification:**
- M? file b?ng Word
- Check n?i dung có ?úng là m?u cam k?t
- Check có th? ?i?n form ???c

---

### ? Test Case 5: Get User Contract

**Steps:**
```bash
curl "http://localhost:5000/api/SalaryContracts/user/38" \
  -H "Authorization: Bearer {token}"
```

**Expected Results:**
- ? HTTP 200 OK
- ? Response ch?a contract info:
  ```json
  {
    "message": "L?y thông tin h?p ??ng thành công",
    "data": {
      "id": 123,
      "userId": 38,
      "baseSalary": 15000000,
      "hasCommitment08": true,
      "attachmentPath": null,
      "createdAt": "2024-01-20T10:00:00Z"
    }
  }
  ```

---

### ? Test Case 6: Upload File Cam K?t

**Steps:**
```bash
curl -X PUT "http://localhost:5000/api/SalaryContracts/123" \
  -H "Authorization: Bearer {user_token}" \
  -H "Content-Type: multipart/form-data" \
  -F "Attachment=@test.pdf"
```

**Expected Results:**
- ? HTTP 200 OK
- ? Response: "C?p nh?t h?p ??ng l??ng thành công"
- ? `AttachmentPath` ???c update
- ? `AttachmentFileName` ???c update
- ? File ???c l?u vào `wwwroot/uploads/salary-contracts/{userName}_{userId}/`
- ? File name là GUID

**Verify File:**
```bash
ls -lh wwwroot/uploads/salary-contracts/Nguyen_Van_A_38/
# Expected: file v?i tên d?ng abc123-guid.pdf
```

---

### ? Test Case 7: Upload File Thay Th? (Xóa File C?)

**Preconditions:**
- Contract ?ã có attachment c?

**Steps:**
```bash
curl -X PUT "http://localhost:5000/api/SalaryContracts/123" \
  -H "Authorization: Bearer {user_token}" \
  -F "Attachment=@new_file.pdf"
```

**Expected Results:**
- ? HTTP 200 OK
- ? File c? b? xóa
- ? File m?i ???c l?u
- ? `AttachmentPath` và `AttachmentFileName` ???c update

**Verify:**
```bash
# File c? không còn t?n t?i
ls wwwroot/uploads/salary-contracts/Nguyen_Van_A_38/old-file.pdf
# Expected: No such file

# File m?i t?n t?i
ls wwwroot/uploads/salary-contracts/Nguyen_Van_A_38/new-guid.pdf
# Expected: File exists
```

---

### ? Test Case 8: Validation - File Extension Không H?p L?

**Steps:**
```bash
curl -X PUT "http://localhost:5000/api/SalaryContracts/123" \
  -H "Authorization: Bearer {user_token}" \
  -F "Attachment=@test.exe"
```

**Expected Results:**
- ? HTTP 400 Bad Request
- ? Response: 
  ```json
  {
    "message": "File không h?p l?. Ch? ch?p nh?n: .pdf, .doc, .docx, .jpg, .jpeg, .png"
  }
  ```

---

### ? Test Case 9: Validation - File Quá L?n

**Steps:**
```bash
# Create 10MB file
dd if=/dev/zero of=large.pdf bs=1M count=10

curl -X PUT "http://localhost:5000/api/SalaryContracts/123" \
  -H "Authorization: Bearer {user_token}" \
  -F "Attachment=@large.pdf"
```

**Expected Results:**
- ? HTTP 400 Bad Request
- ? Response:
  ```json
  {
    "message": "File quá l?n. Kích th??c t?i ?a: 5MB"
  }
  ```

---

### ? Test Case 10: Email Template Không T?n T?i

**Preconditions:**
- Delete template `EMAIL_UPLOAD_08` t? database
  ```sql
  DELETE FROM document_templates WHERE Code = 'EMAIL_UPLOAD_08';
  ```

**Steps:**
```bash
curl -X POST "http://localhost:5000/api/SalaryContracts" \
  -H "Authorization: Bearer {admin_token}" \
  -F "UserId=41" \
  -F "BaseSalary=15000000" \
  -F "HasCommitment08=true"
```

**Expected Results:**
- ? HTTP 201 Created (contract v?n ???c t?o)
- ? Log warning: "Template EMAIL_UPLOAD_08 not found in database."
- ? Email không ???c g?i
- ? Response: "T?o h?p ??ng l??ng thành công" (không crash)

---

### ? Test Case 11: SMTP Configuration Sai

**Preconditions:**
- Set sai SMTP password trong appsettings.json

**Steps:**
```bash
curl -X POST "http://localhost:5000/api/SalaryContracts" \
  -H "Authorization: Bearer {admin_token}" \
  -F "UserId=42" \
  -F "BaseSalary=15000000" \
  -F "HasCommitment08=true"
```

**Expected Results:**
- ? HTTP 201 Created (contract v?n ???c t?o)
- ? Log error: "SMTP error sending commitment notification..."
- ? Email không ???c g?i
- ? Response: "T?o h?p ??ng l??ng thành công" (không crash)

---

### ? Test Case 12: Placeholder Replacement

**Manual Test:**

1. T?o contract
2. Check email nh?n ???c
3. Verify các placeholders ?ã ???c replace ?úng:

| Placeholder | Expected Value |
|-------------|----------------|
| `{{UserName}}` | Tên user t? DB |
| `{{UserEmail}}` | Email user t? DB |
| `{{BaseSalary}}` | Format: 15,000,000 VN? |
| `{{InsuranceSalary}}` | Format: 5,682,000 VN? |
| `{{ContractType}}` | "Chính th?c" ho?c "Vãng lai" |
| `{{DependentsCount}}` | S? ng??i ph? thu?c |
| `{{CreatedAt}}` | Format: dd/MM/yyyy HH:mm:ss |
| `{{UploadAttachmentLink}}` | {FrontendUrl}/circular-08 |
| `{{DownloadTemplateLink}}` | {BackendUrl}/api/.../download-commitment08-template |
| `{{UploadDeadline}}` | CreatedAt + 7 ngày (dd/MM/yyyy) |
| `{{HrEmail}}` | Email HR t? config |
| `{{CurrentYear}}` | 2024 |

---

### ? Test Case 13: Email Links Clickable

**Manual Test:**

1. Nh?n email
2. Click vào button "?? T?i m?u Cam k?t 08"
   - ? File DOCX ???c download
3. Click vào button "?? Upload Cam k?t Thông t? 08 ngay"
   - ? Browser m? trang frontend: `{FrontendUrl}/circular-08`

---

### ? Test Case 14: Multiple Users

**Steps:**
```bash
# User 1
curl -X POST ".../SalaryContracts" -F "UserId=38" -F "HasCommitment08=true"
# User 2
curl -X POST ".../SalaryContracts" -F "UserId=39" -F "HasCommitment08=true"
# User 3
curl -X POST ".../SalaryContracts" -F "UserId=40" -F "HasCommitment08=true"
```

**Expected Results:**
- ? 3 contracts ???c t?o
- ? 3 emails ???c g?i (m?i user 1 email)
- ? M?i email có thông tin ?úng v?i user t??ng ?ng
- ? Links trong email có userId khác nhau

---

### ? Test Case 15: Authorization

**Test 15a: User không th? upload cho contract c?a ng??i khác**
```bash
# User A (ID: 38) c? upload cho contract c?a User B (ID: 39)
curl -X PUT "http://localhost:5000/api/SalaryContracts/456" \
  -H "Authorization: Bearer {userA_token}" \
  -F "Attachment=@file.pdf"
```

**Expected:**
- ? HTTP 403 Forbidden (n?u có check authorization)
- ho?c
- ? HTTP 400 Bad Request

**Test 15b: Anonymous không th? download**
```bash
curl "http://localhost:5000/api/SalaryContracts/download-commitment08-template"
# No Authorization header
```

**Expected:**
- ? HTTP 401 Unauthorized (n?u endpoint có [Authorize])
- ho?c
- ? HTTP 200 OK (n?u endpoint public - theo code hi?n t?i là public)

---

## ?? Logs to Check

### Success Logs
```
[INFO] Preparing to send commitment notification email to user@email.com for user 38
[INFO] Commitment notification email sent successfully to user@email.com for user 38
[INFO] Sent commitment notification email to user 38 for contract 123
```

### Warning Logs
```
[WARN] Email configuration is incomplete. Skipping commitment notification for user 38
[WARN] Template EMAIL_UPLOAD_08 not found in database.
```

### Error Logs
```
[ERROR] SMTP error sending commitment notification for user 38: 5.7.0 - Authentication failure
[ERROR] Error sending commitment notification for user 38
```

---

## ?? Test Coverage

### Functional Tests
- ? Create contract + send email
- ? Email not sent when HasCommitment08 = false
- ? Email not sent when attachment exists
- ? Download template
- ? Upload file
- ? Replace old file
- ? Placeholder replacement
- ? Links in email work

### Validation Tests
- ? Invalid file extension
- ? File too large
- ? Missing template
- ? SMTP error
- ? Authorization

### Edge Cases
- ? Multiple users
- ? User without email
- ? Template missing
- ? SMTP down
- ? Folder permissions

---

## ?? Known Issues to Test

1. **User không có email:**
   - Contract v?n t?o ???c?
   - Log error rõ ràng?

2. **Frontend URL sai:**
   - Link trong email s? 404
   - C?n verify config

3. **SMTP timeout:**
   - Contract t?o có b? ch?m?
   - Có timeout không?

4. **Large scale:**
   - G?i 100 emails cùng lúc?
   - Performance?

---

## ? Regression Tests

Sau m?i l?n deploy, ch?y l?i:

1. ? T?o contract bình th??ng (không có HasCommitment08)
2. ? T?o contract v?i HasCommitment08 = true
3. ? Download template
4. ? Upload file
5. ? Check email nh?n ???c

---

## ?? Test Report Template

```markdown
# Test Report - Email Upload Cam K?t TT08

**Date:** 2024-01-20
**Tester:** [Name]
**Environment:** Development / Staging / Production

## Test Results

| Test Case | Status | Notes |
|-----------|--------|-------|
| TC1: Create + Send Email | ? Pass | Email received in 3s |
| TC2: No Email when false | ? Pass | |
| TC3: No Email with file | ? Pass | |
| TC4: Download Template | ? Pass | File OK |
| TC5: Get User Contract | ? Pass | |
| TC6: Upload File | ? Pass | |
| TC7: Replace File | ? Pass | Old file deleted |
| TC8: Invalid Extension | ? Pass | Error message clear |
| TC9: File Too Large | ? Pass | |
| TC10: Missing Template | ?? Warn | Logged but no crash |
| TC11: SMTP Error | ?? Warn | Logged but no crash |
| TC12: Placeholders | ? Pass | All replaced correctly |
| TC13: Links Clickable | ? Pass | Both links work |
| TC14: Multiple Users | ? Pass | 3/3 emails sent |
| TC15: Authorization | ? Fail | [Details] |

## Issues Found

1. [Issue description]
2. [Issue description]

## Recommendations

1. [Recommendation]
2. [Recommendation]
```

---

## ?? Quick Test Script

```bash
#!/bin/bash

# Test script for Email Upload Commitment 08

BASE_URL="http://localhost:5000"
ADMIN_TOKEN="your_admin_token"
USER_ID=38

echo "?? Starting tests..."

# TC1: Create contract
echo "Test 1: Create contract + send email..."
curl -X POST "$BASE_URL/api/SalaryContracts" \
  -H "Authorization: Bearer $ADMIN_TOKEN" \
  -H "Content-Type: multipart/form-data" \
  -F "UserId=$USER_ID" \
  -F "BaseSalary=15000000" \
  -F "HasCommitment08=true"
echo ""

# TC4: Download template
echo "Test 2: Download template..."
curl -O "$BASE_URL/api/SalaryContracts/download-commitment08-template"
ls -lh Mau_Cam_Ket_Thong_Tu_08.docx
echo ""

# TC5: Get contract
echo "Test 3: Get user contract..."
curl "$BASE_URL/api/SalaryContracts/user/$USER_ID" \
  -H "Authorization: Bearer $ADMIN_TOKEN"
echo ""

echo "? Tests completed!"
```

---

**Happy Testing! ??**
