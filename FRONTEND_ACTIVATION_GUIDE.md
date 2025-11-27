# H??ng d?n Frontend - Kích ho?t tài kho?n v?i Token

## ?? T?ng quan

H? th?ng activation account ?ã ???c tri?n khai hoàn ch?nh v?i các tính n?ng:
- ? Token b?o m?t 256-bit
- ? Token có th?i h?n (24 gi?)
- ? Token ch? s? d?ng 1 l?n
- ? Email t? ??ng khi t?o user
- ? API ?? verify và ??i m?t kh?u

## ?? API Endpoints

### 1. Verify Activation Token
Ki?m tra tính h?p l? c?a activation token

**Endpoint:** `GET /api/Auth/verify-activation-token?token={token}`

**Request:**
```http
GET /api/Auth/verify-activation-token?token=abc123xyz...
```

**Response Success (200):**
```json
{
  "message": "Token h?p l?",
  "user": {
    "email": "user@example.com",
    "name": "Nguy?n V?n A",
    "firstLogin": true
  }
}
```

**Response Error (400):**
```json
{
  "message": "Link kích ho?t không h?p l?"
}
// ho?c
{
  "message": "Link kích ho?t ?ã ???c s? d?ng"
}
// ho?c
{
  "message": "Link kích ho?t ?ã h?t h?n. Vui lòng liên h? qu?n tr? viên ?? ???c h? tr?"
}
```

### 2. Login
??ng nh?p v?i m?t kh?u t?m th?i

**Endpoint:** `POST /api/Auth/login`

**Request:**
```json
{
  "email": "user@example.com",
  "password": "TempPassword123!",
  "deviceInfo": "Chrome on Windows"
}
```

**Response Success (200):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-01T12:00:00Z",
  "firstLogin": true,
  "message": "B?n ph?i ??i m?t kh?u tr??c khi ??ng nh?p vào h? th?ng",
  "user": {
    "id": 1,
    "name": "Nguy?n V?n A",
    "email": "user@example.com",
    "position": "Developer",
    "role": "User",
    "status": "active"
  }
}
```

### 3. Change Password First Time
??i m?t kh?u l?n ??u

**Endpoint:** `POST /api/Auth/change-password-first-time`

**Headers:**
```
Authorization: Bearer {accessToken}
```

**Request:**
```json
{
  "newPassword": "NewSecurePassword123!",
  "confirmPassword": "NewSecurePassword123!"
}
```

**Response Success (200):**
```json
{
  "message": "??i m?t kh?u thành công. B?n có th? ??ng nh?p v?i m?t kh?u m?i"
}
```

**Response Error (400):**
```json
{
  "message": "M?t kh?u ph?i có ít nh?t 8 ký t?"
}
// ho?c
{
  "errors": {
    "ConfirmPassword": ["M?t kh?u xác nh?n không kh?p"]
  }
}
```

## ?? Frontend Implementation

### 1. Route Setup

```typescript
// React Router example
import { Routes, Route } from 'react-router-dom';

<Routes>
  <Route path="/activate-account" element={<ActivateAccountPage />} />
  <Route path="/change-password" element={<ChangePasswordPage />} />
  {/* other routes */}
</Routes>
```

### 2. Activate Account Page Component

```typescript
// ActivateAccountPage.tsx
import React, { useEffect, useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';

interface VerifyTokenResponse {
  message: string;
  user: {
    email: string;
    name: string;
    firstLogin: boolean;
  };
}

const ActivateAccountPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const token = searchParams.get('token');
  
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [userEmail, setUserEmail] = useState('');
  const [userName, setUserName] = useState('');
  const [password, setPassword] = useState('');
  const [step, setStep] = useState<'verifying' | 'login' | 'error'>('verifying');

  useEffect(() => {
    if (token) {
      verifyToken(token);
    } else {
      setError('Link không h?p l?');
      setStep('error');
      setLoading(false);
    }
  }, [token]);

  const verifyToken = async (token: string) => {
    try {
      const response = await fetch(
        `${process.env.REACT_APP_API_URL}/api/Auth/verify-activation-token?token=${encodeURIComponent(token)}`
      );
      
      const data: VerifyTokenResponse = await response.json();
      
      if (response.ok) {
        setUserEmail(data.user.email);
        setUserName(data.user.name);
        setStep('login');
      } else {
        setError(data.message);
        setStep('error');
      }
    } catch (err) {
      setError('L?i k?t n?i ??n server');
      setStep('error');
    } finally {
      setLoading(false);
    }
  };

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    try {
      const response = await fetch(`${process.env.REACT_APP_API_URL}/api/Auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include', // Important for cookies
        body: JSON.stringify({
          email: userEmail,
          password: password,
          deviceInfo: navigator.userAgent
        }),
      });

      const data = await response.json();

      if (response.ok) {
        // L?u access token
        localStorage.setItem('accessToken', data.accessToken);
        localStorage.setItem('user', JSON.stringify(data.user));

        if (data.firstLogin) {
          // Redirect ??n trang ??i m?t kh?u
          navigate('/change-password', { 
            state: { 
              message: data.message,
              mustChangePassword: true 
            } 
          });
        } else {
          // Redirect ??n dashboard
          navigate('/dashboard');
        }
      } else {
        setError(data.message || '??ng nh?p th?t b?i');
      }
    } catch (err) {
      setError('L?i k?t n?i ??n server');
    } finally {
      setLoading(false);
    }
  };

  if (loading && step === 'verifying') {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-4 text-gray-600">?ang xác th?c link kích ho?t...</p>
        </div>
      </div>
    );
  }

  if (step === 'error') {
    return (
      <div className="flex items-center justify-center min-h-screen bg-gray-100">
        <div className="max-w-md w-full bg-white rounded-lg shadow-md p-8">
          <div className="text-center">
            <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-red-100">
              <svg className="h-6 w-6 text-red-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </div>
            <h2 className="mt-4 text-2xl font-bold text-gray-900">Xác th?c th?t b?i</h2>
            <p className="mt-2 text-gray-600">{error}</p>
            <button
              onClick={() => navigate('/login')}
              className="mt-6 w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700"
            >
              Quay v? trang ??ng nh?p
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100">
      <div className="max-w-md w-full bg-white rounded-lg shadow-md p-8">
        <div className="text-center mb-6">
          <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-green-100">
            <svg className="h-6 w-6 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
            </svg>
          </div>
          <h2 className="mt-4 text-2xl font-bold text-gray-900">Kích ho?t tài kho?n</h2>
          <p className="mt-2 text-gray-600">Xin chào, <strong>{userName}</strong></p>
        </div>

        <form onSubmit={handleLogin} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">Email</label>
            <input
              type="email"
              value={userEmail}
              disabled
              className="mt-1 block w-full px-3 py-2 bg-gray-100 border border-gray-300 rounded-md"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">
              M?t kh?u t?m th?i
            </label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              placeholder="Nh?p m?t kh?u t? email"
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
            <p className="mt-1 text-xs text-gray-500">
              M?t kh?u ?ã ???c g?i qua email
            </p>
          </div>

          {error && (
            <div className="bg-red-50 border border-red-200 rounded-md p-3">
              <p className="text-sm text-red-600">{error}</p>
            </div>
          )}

          <button
            type="submit"
            disabled={loading || !password}
            className="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed"
          >
            {loading ? '?ang x? lý...' : '??ng nh?p'}
          </button>
        </form>

        <div className="mt-4 text-center text-sm text-gray-600">
          <p>Sau khi ??ng nh?p, b?n s? ???c yêu c?u ??i m?t kh?u</p>
        </div>
      </div>
    </div>
  );
};

export default ActivateAccountPage;
```

### 3. Change Password Page Component

```typescript
// ChangePasswordPage.tsx
import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';

const ChangePasswordPage: React.FC = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const mustChangePassword = location.state?.mustChangePassword || false;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    // Validation
    if (newPassword.length < 8) {
      setError('M?t kh?u ph?i có ít nh?t 8 ký t?');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('M?t kh?u xác nh?n không kh?p');
      return;
    }

    setLoading(true);

    try {
      const token = localStorage.getItem('accessToken');
      
      const response = await fetch(
        `${process.env.REACT_APP_API_URL}/api/Auth/change-password-first-time`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`,
          },
          body: JSON.stringify({
            newPassword,
            confirmPassword,
          }),
        }
      );

      const data = await response.json();

      if (response.ok) {
        // Hi?n th? thông báo thành công
        alert(data.message);
        
        // Xóa token c? và yêu c?u ??ng nh?p l?i
        localStorage.removeItem('accessToken');
        localStorage.removeItem('user');
        
        // Redirect v? trang login
        navigate('/login', {
          state: {
            message: '??i m?t kh?u thành công. Vui lòng ??ng nh?p l?i v?i m?t kh?u m?i.',
          },
        });
      } else {
        setError(data.message || 'Có l?i x?y ra');
      }
    } catch (err) {
      setError('L?i k?t n?i ??n server');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-100">
      <div className="max-w-md w-full bg-white rounded-lg shadow-md p-8">
        <div className="text-center mb-6">
          <h2 className="text-2xl font-bold text-gray-900">??i m?t kh?u</h2>
          {mustChangePassword && (
            <div className="mt-4 bg-yellow-50 border border-yellow-200 rounded-md p-3">
              <p className="text-sm text-yellow-800">
                ?? B?n ph?i ??i m?t kh?u tr??c khi ti?p t?c s? d?ng h? th?ng
              </p>
            </div>
          )}
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">
              M?t kh?u m?i
            </label>
            <input
              type="password"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              required
              minLength={8}
              placeholder="Nh?p m?t kh?u m?i (t?i thi?u 8 ký t?)"
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">
              Xác nh?n m?t kh?u m?i
            </label>
            <input
              type="password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
              placeholder="Nh?p l?i m?t kh?u m?i"
              className="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-blue-500 focus:border-blue-500"
            />
          </div>

          {error && (
            <div className="bg-red-50 border border-red-200 rounded-md p-3">
              <p className="text-sm text-red-600">{error}</p>
            </div>
          )}

          <div className="bg-blue-50 border border-blue-200 rounded-md p-3">
            <p className="text-xs text-blue-800">
              <strong>Yêu c?u m?t kh?u:</strong>
            </p>
            <ul className="mt-1 text-xs text-blue-700 list-disc list-inside">
              <li>T?i thi?u 8 ký t?</li>
              <li>Nên ch?a ch? hoa, ch? th??ng, s? và ký t? ??c bi?t</li>
            </ul>
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 disabled:bg-gray-400 disabled:cursor-not-allowed"
          >
            {loading ? '?ang x? lý...' : '??i m?t kh?u'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default ChangePasswordPage;
```

### 4. Environment Variables

```env
# .env
REACT_APP_API_URL=http://localhost:5000
```

## ?? Security Best Practices

### 1. Token Storage
- ? Access Token: L?u trong localStorage ho?c memory
- ? Refresh Token: ???c l?u trong HttpOnly cookie (backend t? ??ng)

### 2. Password Requirements
```typescript
const validatePassword = (password: string): string[] => {
  const errors: string[] = [];
  
  if (password.length < 8) {
    errors.push('M?t kh?u ph?i có ít nh?t 8 ký t?');
  }
  
  if (!/[A-Z]/.test(password)) {
    errors.push('M?t kh?u ph?i ch?a ít nh?t 1 ch? hoa');
  }
  
  if (!/[a-z]/.test(password)) {
    errors.push('M?t kh?u ph?i ch?a ít nh?t 1 ch? th??ng');
  }
  
  if (!/[0-9]/.test(password)) {
    errors.push('M?t kh?u ph?i ch?a ít nh?t 1 s?');
  }
  
  if (!/[!@#$%^&*]/.test(password)) {
    errors.push('M?t kh?u ph?i ch?a ít nh?t 1 ký t? ??c bi?t');
  }
  
  return errors;
};
```

### 3. Error Handling
```typescript
const handleApiError = (error: any) => {
  if (error.response) {
    // Server responded with error
    return error.response.data.message || 'Có l?i x?y ra';
  } else if (error.request) {
    // No response received
    return 'Không th? k?t n?i ??n server';
  } else {
    // Request setup error
    return 'L?i khi g?i yêu c?u';
  }
};
```

## ?? Mobile Responsive

```css
/* Tailwind CSS classes for mobile responsive */
.container {
  @apply max-w-md mx-auto px-4 sm:px-6 lg:px-8;
}

.form-input {
  @apply w-full px-3 py-2 text-sm sm:text-base;
}

.button {
  @apply w-full py-2 px-4 text-sm sm:text-base;
}
```

## ?? Testing

### Unit Test Example (Jest + React Testing Library)

```typescript
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import ActivateAccountPage from './ActivateAccountPage';

describe('ActivateAccountPage', () => {
  it('should verify token on mount', async () => {
    global.fetch = jest.fn(() =>
      Promise.resolve({
        ok: true,
        json: () => Promise.resolve({
          message: 'Token h?p l?',
          user: {
            email: 'test@example.com',
            name: 'Test User',
            firstLogin: true
          }
        }),
      })
    ) as jest.Mock;

    render(
      <BrowserRouter>
        <ActivateAccountPage />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(screen.getByText(/Test User/i)).toBeInTheDocument();
    });
  });
});
```

## ?? Support

N?u có v?n ??, ki?m tra:
1. Backend API ?ang ch?y
2. CORS ???c c?u hình ?úng
3. Token ch?a h?t h?n
4. Network requests trong DevTools

## ?? Checklist Tri?n khai

- [ ] T?o routes `/activate-account` và `/change-password`
- [ ] Implement ActivateAccountPage component
- [ ] Implement ChangePasswordPage component
- [ ] Configure environment variables
- [ ] Test v?i token h?p l?
- [ ] Test v?i token h?t h?n
- [ ] Test v?i token ?ã dùng
- [ ] Test validation m?t kh?u
- [ ] Test responsive trên mobile
- [ ] Deploy lên production
