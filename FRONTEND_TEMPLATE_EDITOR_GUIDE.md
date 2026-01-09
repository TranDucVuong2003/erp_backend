# ?? Frontend Quick Start: Template Editor

## ?? Overview

H??ng d?n tích h?p Template Editor vào React/Next.js app ?? t?o HTML template v?i dynamic placeholders.

---

## ?? Installation

```bash
npm install @tinymce/tinymce-react
# ho?c
npm install react-quill
# ho?c
npm install @ckeditor/ckeditor5-react
```

---

## ?? React Component Example

### **TemplateEditor.tsx**

```typescript
import React, { useState, useEffect } from 'react';
import { Editor } from '@tinymce/tinymce-react';
import axios from 'axios';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

interface Placeholder {
  name: string;
  description?: string;
}

const TemplateEditor: React.FC = () => {
  const [htmlContent, setHtmlContent] = useState('');
  const [templateName, setTemplateName] = useState('');
  const [templateCode, setTemplateCode] = useState('');
  const [templateType, setTemplateType] = useState('salary_notification');
  const [detectedPlaceholders, setDetectedPlaceholders] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [previewHtml, setPreviewHtml] = useState('');

  // Common placeholders library
  const commonPlaceholders: Placeholder[] = [
    { name: 'EmployeeName', description: 'Tên nhân viên' },
    { name: 'BaseSalary', description: 'L??ng c? b?n' },
    { name: 'NetSalary', description: 'L??ng th?c nh?n' },
    { name: 'Month', description: 'Tháng (MM/YYYY)' },
    { name: 'Department', description: 'Phòng ban' },
    { name: 'CurrentDate', description: 'Ngày hi?n t?i' },
    { name: 'CompanyName', description: 'Tên công ty' },
  ];

  // Auto-detect placeholders when HTML changes
  useEffect(() => {
    const detectPlaceholders = async () => {
      if (!htmlContent) return;

      try {
        const response = await axios.post(
          `${API_URL}/api/DocumentTemplates/extract-placeholders`,
          { htmlContent },
          {
            headers: {
              'Authorization': `Bearer ${localStorage.getItem('token')}`
            }
          }
        );

        setDetectedPlaceholders(response.data.placeholders);
      } catch (error) {
        console.error('Error detecting placeholders:', error);
      }
    };

    const debounce = setTimeout(detectPlaceholders, 500);
    return () => clearTimeout(debounce);
  }, [htmlContent]);

  // Insert placeholder at cursor
  const insertPlaceholder = (placeholderName: string) => {
    const editor = (window as any).tinymce?.activeEditor;
    if (editor) {
      editor.insertContent(`{{${placeholderName}}}`);
    }
  };

  // Preview with sample data
  const handlePreview = async () => {
    const sampleData: Record<string, string> = {};
    
    detectedPlaceholders.forEach(placeholder => {
      switch (placeholder.toLowerCase()) {
        case 'employeename':
          sampleData[placeholder] = 'Nguy?n V?n A';
          break;
        case 'basesalary':
          sampleData[placeholder] = '15,000,000';
          break;
        case 'netsalary':
          sampleData[placeholder] = '13,500,000';
          break;
        case 'month':
          sampleData[placeholder] = '01/2026';
          break;
        case 'department':
          sampleData[placeholder] = 'IT Department';
          break;
        case 'currentdate':
          sampleData[placeholder] = new Date().toLocaleDateString('vi-VN');
          break;
        default:
          sampleData[placeholder] = `[Sample ${placeholder}]`;
      }
    });

    try {
      // T?m l?u template ?? l?y ID (ho?c dùng render tr?c ti?p)
      const createResponse = await axios.post(
        `${API_URL}/api/DocumentTemplates`,
        {
          name: `Preview_${Date.now()}`,
          templateType: 'preview',
          code: `PREVIEW_${Date.now()}`,
          htmlContent: htmlContent,
          isActive: false
        },
        {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        }
      );

      const templateId = createResponse.data.data.id;

      // Render v?i sample data
      const renderResponse = await axios.post(
        `${API_URL}/api/DocumentTemplates/${templateId}/render`,
        sampleData,
        {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          },
          responseType: 'text'
        }
      );

      setPreviewHtml(renderResponse.data);

      // Xóa preview template
      await axios.delete(
        `${API_URL}/api/DocumentTemplates/${templateId}`,
        {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        }
      );
    } catch (error) {
      console.error('Error previewing:', error);
      alert('L?i khi preview template');
    }
  };

  // Save template
  const handleSave = async () => {
    if (!templateName || !templateCode || !htmlContent) {
      alert('Vui lòng ?i?n ??y ?? thông tin!');
      return;
    }

    setIsLoading(true);

    try {
      const response = await axios.post(
        `${API_URL}/api/DocumentTemplates`,
        {
          name: templateName,
          templateType: templateType,
          code: templateCode.toUpperCase(),
          htmlContent: htmlContent,
          description: `Template t?o ngày ${new Date().toLocaleDateString('vi-VN')}`,
          availablePlaceholders: JSON.stringify(detectedPlaceholders),
          isDefault: false
        },
        {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`
          }
        }
      );

      alert('? T?o template thành công!');
      console.log('Template created:', response.data);
      
      // Reset form
      setTemplateName('');
      setTemplateCode('');
      setHtmlContent('');
      setDetectedPlaceholders([]);
    } catch (error: any) {
      console.error('Error saving template:', error);
      alert(`? L?i: ${error.response?.data?.message || error.message}`);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="container mx-auto p-6">
      <h1 className="text-3xl font-bold mb-6">?? Template Editor</h1>

      <div className="grid grid-cols-3 gap-6">
        {/* Left: Editor */}
        <div className="col-span-2">
          <div className="mb-4 space-y-3">
            <input
              type="text"
              placeholder="Tên template (VD: Thông báo l??ng V2)"
              value={templateName}
              onChange={(e) => setTemplateName(e.target.value)}
              className="w-full px-4 py-2 border rounded-lg"
            />
            
            <input
              type="text"
              placeholder="Code (VD: SALARY_NOTIFY_V2)"
              value={templateCode}
              onChange={(e) => setTemplateCode(e.target.value)}
              className="w-full px-4 py-2 border rounded-lg"
            />

            <select
              value={templateType}
              onChange={(e) => setTemplateType(e.target.value)}
              className="w-full px-4 py-2 border rounded-lg"
            >
              <option value="salary_notification">Thông báo l??ng</option>
              <option value="contract">H?p ??ng</option>
              <option value="email">Email</option>
              <option value="report">Báo cáo</option>
            </select>
          </div>

          <Editor
            apiKey="your-tinymce-api-key" // Get free key at tinymce.com
            value={htmlContent}
            onEditorChange={setHtmlContent}
            init={{
              height: 600,
              menubar: true,
              plugins: [
                'advlist', 'autolink', 'lists', 'link', 'image', 'charmap',
                'anchor', 'searchreplace', 'visualblocks', 'code', 'fullscreen',
                'insertdatetime', 'media', 'table', 'preview', 'help', 'wordcount'
              ],
              toolbar: 'undo redo | blocks | ' +
                'bold italic forecolor | alignleft aligncenter ' +
                'alignright alignjustify | bullist numlist outdent indent | ' +
                'removeformat | code | help',
              content_style: 'body { font-family:Arial,sans-serif; font-size:14px }'
            }}
          />

          <div className="mt-4 flex gap-3">
            <button
              onClick={handlePreview}
              disabled={!htmlContent || detectedPlaceholders.length === 0}
              className="px-6 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 disabled:opacity-50"
            >
              ??? Preview
            </button>
            
            <button
              onClick={handleSave}
              disabled={isLoading || !htmlContent}
              className="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50"
            >
              {isLoading ? '? ?ang l?u...' : '?? L?u Template'}
            </button>
          </div>
        </div>

        {/* Right: Placeholders */}
        <div className="space-y-6">
          {/* Common Placeholders */}
          <div className="bg-white p-4 rounded-lg shadow">
            <h3 className="font-bold text-lg mb-3">?? Placeholders Thông D?ng</h3>
            <div className="space-y-2">
              {commonPlaceholders.map((p) => (
                <button
                  key={p.name}
                  onClick={() => insertPlaceholder(p.name)}
                  className="w-full text-left px-3 py-2 bg-blue-50 hover:bg-blue-100 rounded text-sm"
                  title={p.description}
                >
                  <code>{`{{${p.name}}}`}</code>
                  {p.description && (
                    <span className="block text-xs text-gray-500 mt-1">
                      {p.description}
                    </span>
                  )}
                </button>
              ))}
            </div>
          </div>

          {/* Detected Placeholders */}
          <div className="bg-green-50 p-4 rounded-lg shadow">
            <h3 className="font-bold text-lg mb-3">
              ?? ?ã phát hi?n ({detectedPlaceholders.length})
            </h3>
            {detectedPlaceholders.length === 0 ? (
              <p className="text-sm text-gray-500">
                Ch?a có placeholder nào. S? d?ng syntax <code>{`{{VariableName}}`}</code>
              </p>
            ) : (
              <div className="space-y-1">
                {detectedPlaceholders.map((p) => (
                  <div key={p} className="text-sm text-green-700 font-mono">
                    ? {`{{${p}}}`}
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Usage Guide */}
          <div className="bg-yellow-50 p-4 rounded-lg shadow">
            <h3 className="font-bold text-lg mb-2">?? H??ng d?n</h3>
            <ul className="text-sm space-y-1 text-gray-700">
              <li>• Nh?n vào placeholder ?? chèn</li>
              <li>• Ho?c gõ <code>{`{{TenBien}}`}</code></li>
              <li>• T? ??ng phát hi?n placeholders</li>
              <li>• Preview ?? ki?m tra k?t qu?</li>
            </ul>
          </div>
        </div>
      </div>

      {/* Preview Modal */}
      {previewHtml && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-4xl w-full max-h-[90vh] overflow-auto">
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-2xl font-bold">??? Preview</h2>
              <button
                onClick={() => setPreviewHtml('')}
                className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300"
              >
                ?óng
              </button>
            </div>
            <iframe
              srcDoc={previewHtml}
              className="w-full h-[600px] border rounded"
              title="Template Preview"
            />
          </div>
        </div>
      )}
    </div>
  );
};

export default TemplateEditor;
```

---

## ?? API Service Helper

### **templateService.ts**

```typescript
import axios from 'axios';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

export interface DocumentTemplate {
  id: number;
  name: string;
  templateType: string;
  code: string;
  htmlContent: string;
  description?: string;
  availablePlaceholders?: string;
  version: number;
  isActive: boolean;
  isDefault: boolean;
  createdAt: string;
}

export interface CreateTemplateRequest {
  name: string;
  templateType: string;
  code: string;
  htmlContent: string;
  description?: string;
  availablePlaceholders?: string;
  isDefault?: boolean;
}

class TemplateService {
  private getAuthHeaders() {
    const token = localStorage.getItem('token');
    return {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    };
  }

  // T?o template m?i
  async createTemplate(data: CreateTemplateRequest): Promise<DocumentTemplate> {
    const response = await axios.post(
      `${API_URL}/api/DocumentTemplates`,
      data,
      { headers: this.getAuthHeaders() }
    );
    return response.data.data;
  }

  // L?y t?t c? templates
  async getTemplates(type?: string): Promise<DocumentTemplate[]> {
    const url = type 
      ? `${API_URL}/api/DocumentTemplates?type=${type}`
      : `${API_URL}/api/DocumentTemplates`;
    
    const response = await axios.get(url, {
      headers: this.getAuthHeaders()
    });
    return response.data.data;
  }

  // L?y template theo ID
  async getTemplate(id: number): Promise<DocumentTemplate> {
    const response = await axios.get(
      `${API_URL}/api/DocumentTemplates/${id}`,
      { headers: this.getAuthHeaders() }
    );
    return response.data.data;
  }

  // L?y template v?i auto-detected placeholders
  async getTemplateWithPlaceholders(id: number) {
    const response = await axios.get(
      `${API_URL}/api/DocumentTemplates/${id}/with-placeholders`,
      { headers: this.getAuthHeaders() }
    );
    return response.data;
  }

  // Extract placeholders t? HTML
  async extractPlaceholders(htmlContent: string): Promise<string[]> {
    const response = await axios.post(
      `${API_URL}/api/DocumentTemplates/extract-placeholders`,
      { htmlContent },
      { headers: this.getAuthHeaders() }
    );
    return response.data.placeholders;
  }

  // Render template v?i data
  async renderTemplate(
    id: number, 
    data: Record<string, string>
  ): Promise<string> {
    const response = await axios.post(
      `${API_URL}/api/DocumentTemplates/${id}/render`,
      data,
      {
        headers: this.getAuthHeaders(),
        responseType: 'text'
      }
    );
    return response.data;
  }

  // Render template theo code
  async renderTemplateByCode(
    code: string,
    data: Record<string, string>
  ): Promise<string> {
    const response = await axios.post(
      `${API_URL}/api/DocumentTemplates/render-by-code/${code}`,
      data,
      {
        headers: this.getAuthHeaders(),
        responseType: 'text'
      }
    );
    return response.data;
  }

  // Validate template data
  async validateTemplateData(
    id: number,
    data: Record<string, string>
  ) {
    const response = await axios.post(
      `${API_URL}/api/DocumentTemplates/${id}/validate`,
      data,
      { headers: this.getAuthHeaders() }
    );
    return response.data;
  }

  // Update template
  async updateTemplate(id: number, data: Partial<CreateTemplateRequest>) {
    const response = await axios.put(
      `${API_URL}/api/DocumentTemplates/${id}`,
      data,
      { headers: this.getAuthHeaders() }
    );
    return response.data.data;
  }

  // Delete template
  async deleteTemplate(id: number) {
    await axios.delete(
      `${API_URL}/api/DocumentTemplates/${id}`,
      { headers: this.getAuthHeaders() }
    );
  }
}

export const templateService = new TemplateService();
```

---

## ?? Template List Component

### **TemplateList.tsx**

```typescript
import React, { useState, useEffect } from 'react';
import { templateService, DocumentTemplate } from './templateService';

const TemplateList: React.FC = () => {
  const [templates, setTemplates] = useState<DocumentTemplate[]>([]);
  const [selectedType, setSelectedType] = useState<string>('all');
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    loadTemplates();
  }, [selectedType]);

  const loadTemplates = async () => {
    setIsLoading(true);
    try {
      const data = selectedType === 'all' 
        ? await templateService.getTemplates()
        : await templateService.getTemplates(selectedType);
      setTemplates(data);
    } catch (error) {
      console.error('Error loading templates:', error);
      alert('L?i khi t?i danh sách templates');
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('B?n có ch?c mu?n xóa template này?')) return;

    try {
      await templateService.deleteTemplate(id);
      alert('? Xóa template thành công');
      loadTemplates();
    } catch (error) {
      console.error('Error deleting template:', error);
      alert('? L?i khi xóa template');
    }
  };

  if (isLoading) {
    return <div className="text-center py-10">? ?ang t?i...</div>;
  }

  return (
    <div className="container mx-auto p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">?? Danh sách Templates</h1>
        
        <select
          value={selectedType}
          onChange={(e) => setSelectedType(e.target.value)}
          className="px-4 py-2 border rounded-lg"
        >
          <option value="all">T?t c?</option>
          <option value="salary_notification">Thông báo l??ng</option>
          <option value="contract">H?p ??ng</option>
          <option value="email">Email</option>
          <option value="report">Báo cáo</option>
        </select>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {templates.map((template) => (
          <div
            key={template.id}
            className="bg-white border rounded-lg p-4 shadow hover:shadow-lg transition"
          >
            <div className="flex justify-between items-start mb-3">
              <h3 className="font-bold text-lg">{template.name}</h3>
              {template.isDefault && (
                <span className="bg-green-100 text-green-800 text-xs px-2 py-1 rounded">
                  Default
                </span>
              )}
            </div>

            <p className="text-sm text-gray-600 mb-2">
              Code: <code className="bg-gray-100 px-2 py-1 rounded">{template.code}</code>
            </p>

            <p className="text-sm text-gray-600 mb-2">
              Type: {template.templateType}
            </p>

            <p className="text-sm text-gray-600 mb-3">
              Version: {template.version}
            </p>

            {template.description && (
              <p className="text-sm text-gray-500 mb-3 italic">
                {template.description}
              </p>
            )}

            <div className="flex gap-2">
              <button
                onClick={() => window.open(`/templates/${template.id}/edit`, '_blank')}
                className="flex-1 px-3 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 text-sm"
              >
                ?? S?a
              </button>
              
              <button
                onClick={() => handleDelete(template.id)}
                className="flex-1 px-3 py-2 bg-red-500 text-white rounded hover:bg-red-600 text-sm"
              >
                ??? Xóa
              </button>
            </div>
          </div>
        ))}
      </div>

      {templates.length === 0 && (
        <div className="text-center py-10 text-gray-500">
          Không có template nào
        </div>
      )}
    </div>
  );
};

export default TemplateList;
```

---

## ?? Usage Examples

### **G?i Email v?i Template**

```typescript
import { templateService } from './templateService';

async function sendSalaryNotificationEmail(userId: number, month: number, year: number) {
  // 1. L?y data t? API
  const employee = await fetchEmployee(userId);
  const payslip = await fetchPayslip(userId, month, year);

  // 2. Chu?n b? data
  const templateData = {
    EmployeeName: employee.name,
    BaseSalary: formatCurrency(employee.baseSalary),
    NetSalary: formatCurrency(payslip.netSalary),
    Month: `${month.toString().padStart(2, '0')}/${year}`,
    Department: employee.department,
    CurrentDate: new Date().toLocaleDateString('vi-VN')
  };

  // 3. Render template
  const emailHtml = await templateService.renderTemplateByCode(
    'SALARY_NOTIFY_V2',
    templateData
  );

  // 4. G?i email
  await sendEmail(employee.email, `Thông báo l??ng tháng ${month}/${year}`, emailHtml);
}
```

---

## ? Testing Checklist

- [ ] Có th? t?o template m?i
- [ ] Auto-detect placeholders ho?t ??ng
- [ ] Preview hi?n th? ?úng
- [ ] L?u template thành công
- [ ] Render v?i real data
- [ ] Validate data tr??c khi render
- [ ] Xóa/S?a template
- [ ] Handle errors gracefully

---

**Bây gi? b?n có th? t? t?o template trong UI mà không c?n s?a backend!** ??
