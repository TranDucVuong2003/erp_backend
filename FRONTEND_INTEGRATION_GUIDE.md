# ?? H??ng D?n Frontend Integration - Placeholder Schema System

## ?? T?ng Quan

Tài li?u này h??ng d?n frontend developers tích h?p **Placeholder Schema System** m?i vào React/Next.js application.

### ?? M?c Tiêu

Thay vì qu?n lý placeholders theo ki?u flat không có c?u trúc, gi? b?n s? có:
- ? **Placeholders ???c nhóm theo entities** (Contract, Customer, SaleOrder...)
- ? **Autocomplete thông minh** khi gõ `{{Entity.`
- ? **Validation tr??c khi l?u** template
- ? **Type-aware suggestions** (bi?t field nào là string, number, date...)

---

## ??? Ki?n Trúc M?i

```
???????????????????????????????????????????????????????????
?              TEMPLATE EDITOR COMPONENT                   ?
???????????????????????????????????????????????????????????
?                                                           ?
?  ??????????????????????      ?????????????????????????? ?
?  ?  HTML Editor       ?      ?  PlaceholderSelector   ? ?
?  ?  (textarea/monaco) ????????  - Entity tabs         ? ?
?  ?                    ?      ?  - Search              ? ?
?  ?  Support:          ?      ?  - Click to insert     ? ?
?  ?  - {{Entity.Field}}?      ?  - Type badges         ? ?
?  ?  - Autocomplete    ?      ?????????????????????????? ?
?  ?  - Validation      ?                 ?               ?
?  ??????????????????????                 ?               ?
?           ?                              ?               ?
?           ????????????????????????????????               ?
?                          ?                               ?
????????????????????????????????????????????????????????????
                           ?
                           ?
              ???????????????????????????
              ?   TemplateService       ?
              ?  - getSchema()          ?
              ?  - validate()           ?
              ?  - render()             ?
              ???????????????????????????
                           ?
                           ?
              ???????????????????????????
              ?   Backend API           ?
              ?  - Schema endpoints     ?
              ?  - Render endpoints     ?
              ???????????????????????????
```

---

## ?? Step 1: Setup Dependencies

### Install Required Packages

```bash
npm install axios
# ho?c
yarn add axios
```

---

## ?? Step 2: T?o Template Service

T?o file `src/services/templateService.ts`:

```typescript
import axios, { AxiosInstance } from 'axios';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

// ============================================================
// Types & Interfaces
// ============================================================

export interface PlaceholderField {
  name: string;
  placeholder: string;
  type: 'string' | 'number' | 'date' | 'boolean';
  description: string;
  isRequired: boolean;
  example: string;
}

export interface EntityPlaceholders {
  [entityName: string]: PlaceholderField[];
}

export interface DocumentTemplate {
  id: number;
  name: string;
  templateType: string;
  code: string;
  htmlContent: string;
  description?: string;
  version: number;
  isActive: boolean;
  isDefault: boolean;
  createdAt: string;
}

export interface TemplateWithPlaceholders {
  template: DocumentTemplate;
  detectedPlaceholders: string[];
  placeholderCount: number;
}

export interface ValidationResult {
  success: boolean;
  message: string;
  isValid: boolean;
  missingPlaceholders?: string[];
  invalidPlaceholders?: string[];
}

// ============================================================
// Template Service Class
// ============================================================

class TemplateService {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: API_URL,
      headers: {
        'Content-Type': 'application/json'
      }
    });

    // Intercept requests to add auth token
    this.api.interceptors.request.use((config) => {
      const token = localStorage.getItem('token');
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });
  }

  // ============================================================
  // ?? NEW: SCHEMA APIs
  // ============================================================

  /**
   * L?y available placeholders theo template type
   * @param templateType - 'contract', 'quote', 'invoice', etc.
   */
  async getAvailablePlaceholders(templateType: string): Promise<EntityPlaceholders> {
    try {
      const response = await this.api.get('/api/DocumentTemplates/schema/placeholders', {
        params: { templateType }
      });
      return response.data.data;
    } catch (error: any) {
      console.error('Error fetching placeholders:', error);
      throw new Error(error.response?.data?.message || 'Không th? t?i danh sách placeholders');
    }
  }

  /**
   * L?y placeholders c?a m?t entity c? th?
   * @param entityName - 'Contract', 'Customer', 'SaleOrder', etc.
   */
  async getPlaceholdersForEntity(entityName: string): Promise<PlaceholderField[]> {
    try {
      const response = await this.api.get(`/api/DocumentTemplates/schema/placeholders/${entityName}`);
      return response.data.placeholders;
    } catch (error: any) {
      console.error(`Error fetching placeholders for ${entityName}:`, error);
      throw new Error(error.response?.data?.message || `Không th? t?i placeholders c?a ${entityName}`);
    }
  }

  /**
   * L?y danh sách t?t c? entities
   */
  async getAvailableEntities(): Promise<string[]> {
    try {
      const response = await this.api.get('/api/DocumentTemplates/schema/entities');
      return response.data.entities;
    } catch (error: any) {
      console.error('Error fetching entities:', error);
      throw new Error(error.response?.data?.message || 'Không th? t?i danh sách entities');
    }
  }

  /**
   * Validate placeholders có h?p l? v?i template type không
   */
  async validatePlaceholderSchema(
    placeholders: string[],
    templateType: string
  ): Promise<ValidationResult> {
    try {
      const response = await this.api.post('/api/DocumentTemplates/schema/validate-placeholders', {
        placeholders,
        templateType
      });
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 400) {
        return error.response.data;
      }
      console.error('Error validating placeholders:', error);
      throw new Error(error.response?.data?.message || 'L?i khi validate placeholders');
    }
  }

  // ============================================================
  // ?? NEW: RENDER v?i Object Data
  // ============================================================

  /**
   * Render template v?i structured object data
   */
  async renderTemplateWithObject(
    templateId: number,
    data: Record<string, any>
  ): Promise<string> {
    try {
      const response = await this.api.post(
        `/api/DocumentTemplates/render-with-object/${templateId}`,
        data,
        { responseType: 'text' }
      );
      return response.data;
    } catch (error: any) {
      console.error('Error rendering template:', error);
      throw new Error(error.response?.data?.message || 'L?i khi render template');
    }
  }

  /**
   * Render template by code v?i object data
   */
  async renderTemplateWithObjectByCode(
    templateCode: string,
    data: Record<string, any>
  ): Promise<string> {
    try {
      const response = await this.api.post(
        `/api/DocumentTemplates/render-with-object-by-code/${templateCode}`,
        data,
        { responseType: 'text' }
      );
      return response.data;
    } catch (error: any) {
      console.error('Error rendering template:', error);
      throw new Error(error.response?.data?.message || 'L?i khi render template');
    }
  }

  // ============================================================
  // OLD APIs (Still supported)
  // ============================================================

  /**
   * T? ??ng phát hi?n placeholders t? HTML
   */
  async extractPlaceholders(htmlContent: string): Promise<string[]> {
    try {
      const response = await this.api.post('/api/DocumentTemplates/extract-placeholders', {
        htmlContent
      });
      return response.data.placeholders;
    } catch (error: any) {
      console.error('Error extracting placeholders:', error);
      throw new Error(error.response?.data?.message || 'L?i khi extract placeholders');
    }
  }

  /**
   * Validate data tr??c khi render
   */
  async validateTemplateData(
    templateId: number,
    data: Record<string, string>
  ): Promise<ValidationResult> {
    try {
      const response = await this.api.post(`/api/DocumentTemplates/validate/${templateId}`, data);
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 400) {
        return error.response.data;
      }
      console.error('Error validating template data:', error);
      throw new Error(error.response?.data?.message || 'L?i khi validate data');
    }
  }

  /**
   * Render template v?i flat dictionary (OLD - still supported)
   */
  async renderTemplate(
    templateId: number,
    data: Record<string, string>
  ): Promise<string> {
    try {
      const response = await this.api.post(
        `/api/DocumentTemplates/render/${templateId}`,
        data,
        { responseType: 'text' }
      );
      return response.data;
    } catch (error: any) {
      console.error('Error rendering template:', error);
      throw new Error(error.response?.data?.message || 'L?i khi render template');
    }
  }

  /**
   * L?y template kèm placeholders
   */
  async getTemplateWithPlaceholders(templateId: number): Promise<TemplateWithPlaceholders> {
    try {
      const response = await this.api.get(`/api/DocumentTemplates/with-placeholders/${templateId}`);
      return response.data.data;
    } catch (error: any) {
      console.error('Error fetching template:', error);
      throw new Error(error.response?.data?.message || 'L?i khi t?i template');
    }
  }

  /**
   * T?o template m?i
   */
  async createTemplate(template: Partial<DocumentTemplate>): Promise<DocumentTemplate> {
    try {
      const response = await this.api.post('/api/DocumentTemplates', template);
      return response.data.data;
    } catch (error: any) {
      console.error('Error creating template:', error);
      throw new Error(error.response?.data?.message || 'L?i khi t?o template');
    }
  }

  /**
   * C?p nh?t template
   */
  async updateTemplate(
    templateId: number,
    template: Partial<DocumentTemplate>
  ): Promise<DocumentTemplate> {
    try {
      const response = await this.api.put(`/api/DocumentTemplates/${templateId}`, template);
      return response.data.data;
    } catch (error: any) {
      console.error('Error updating template:', error);
      throw new Error(error.response?.data?.message || 'L?i khi c?p nh?t template');
    }
  }

  /**
   * L?y danh sách templates
   */
  async getTemplates(templateType?: string): Promise<DocumentTemplate[]> {
    try {
      const response = await this.api.get('/api/DocumentTemplates', {
        params: templateType ? { type: templateType } : undefined
      });
      return response.data.data;
    } catch (error: any) {
      console.error('Error fetching templates:', error);
      throw new Error(error.response?.data?.message || 'L?i khi t?i danh sách templates');
    }
  }
}

// Export singleton instance
export const templateService = new TemplateService();
```

---

## ?? Step 3: T?o PlaceholderSelector Component

T?o file `src/components/PlaceholderSelector.tsx`:

```typescript
import React, { useState, useEffect } from 'react';
import { templateService, PlaceholderField, EntityPlaceholders } from '@/services/templateService';
import './PlaceholderSelector.css'; // CSS ? b??c sau

interface Props {
  templateType: string;
  onInsertPlaceholder: (placeholder: string) => void;
  onClose?: () => void;
}

const PlaceholderSelector: React.FC<Props> = ({
  templateType,
  onInsertPlaceholder,
  onClose
}) => {
  const [placeholders, setPlaceholders] = useState<EntityPlaceholders>({});
  const [selectedEntity, setSelectedEntity] = useState<string>('');
  const [searchTerm, setSearchTerm] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    fetchPlaceholders();
  }, [templateType]);

  const fetchPlaceholders = async () => {
    setLoading(true);
    setError(null);

    try {
      const data = await templateService.getAvailablePlaceholders(templateType);
      setPlaceholders(data);

      // Auto-select first entity
      const firstEntity = Object.keys(data)[0];
      if (firstEntity) {
        setSelectedEntity(firstEntity);
      }
    } catch (err: any) {
      console.error('Error fetching placeholders:', err);
      setError(err.message || 'L?i khi t?i placeholders');
    } finally {
      setLoading(false);
    }
  };

  const filteredPlaceholders = selectedEntity && placeholders[selectedEntity]
    ? placeholders[selectedEntity].filter(p =>
        p.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        p.placeholder.toLowerCase().includes(searchTerm.toLowerCase()) ||
        p.description.toLowerCase().includes(searchTerm.toLowerCase())
      )
    : [];

  const handleInsert = (placeholder: string) => {
    onInsertPlaceholder(placeholder);
  };

  if (loading) {
    return (
      <div className="placeholder-selector loading">
        <div className="spinner">? ?ang t?i...</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="placeholder-selector error">
        <div className="error-message">
          ? {error}
          <button onClick={fetchPlaceholders} className="retry-btn">
            Th? l?i
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="placeholder-selector">
      {/* Header */}
      <div className="selector-header">
        <h3>?? Chèn Bi?n ??ng</h3>
        {onClose && (
          <button className="close-btn" onClick={onClose} aria-label="?óng">
            ?
          </button>
        )}
      </div>

      {/* Entity Tabs */}
      <div className="entity-tabs">
        {Object.entries(placeholders).map(([entity, fields]) => (
          <button
            key={entity}
            className={`tab ${selectedEntity === entity ? 'active' : ''}`}
            onClick={() => setSelectedEntity(entity)}
          >
            {entity} <span className="count">({fields.length})</span>
          </button>
        ))}
      </div>

      {/* Search */}
      <div className="search-section">
        <input
          type="text"
          placeholder="?? Tìm ki?m placeholder..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="search-input"
        />
      </div>

      {/* Placeholder List */}
      <div className="placeholder-list">
        {filteredPlaceholders.length === 0 ? (
          <div className="empty-state">
            {searchTerm ? 'Không tìm th?y placeholder phù h?p' : 'Không có placeholder nào'}
          </div>
        ) : (
          filteredPlaceholders.map(field => (
            <div
              key={field.name}
              className="placeholder-item"
              onClick={() => handleInsert(field.placeholder)}
              title={`Click ?? chèn: ${field.placeholder}`}
            >
              <div className="item-header">
                <code className="placeholder-code">{field.placeholder}</code>
                {field.isRequired && (
                  <span className="required-badge">B?t bu?c</span>
                )}
              </div>
              <div className="item-meta">
                <span className={`type-badge type-${field.type}`}>
                  {field.type}
                </span>
                <span className="example">
                  VD: <strong>{field.example}</strong>
                </span>
              </div>
            </div>
          ))
        )}
      </div>

      {/* Footer */}
      <div className="selector-footer">
        <small>?? Click vào placeholder ?? chèn vào template</small>
      </div>
    </div>
  );
};

export default PlaceholderSelector;
```

---

## ?? Step 4: CSS cho PlaceholderSelector

T?o file `src/components/PlaceholderSelector.css`:

```css
.placeholder-selector {
  width: 400px;
  max-height: 600px;
  background: white;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* Header */
.selector-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  border-bottom: 1px solid #e0e0e0;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
}

.selector-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
}

.close-btn {
  background: transparent;
  border: none;
  color: white;
  font-size: 24px;
  cursor: pointer;
  padding: 0;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  transition: background 0.2s;
}

.close-btn:hover {
  background: rgba(255, 255, 255, 0.2);
}

/* Entity Tabs */
.entity-tabs {
  display: flex;
  overflow-x: auto;
  border-bottom: 1px solid #e0e0e0;
  background: #f5f5f5;
}

.tab {
  padding: 12px 16px;
  border: none;
  background: transparent;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  color: #666;
  transition: all 0.2s;
  white-space: nowrap;
  border-bottom: 2px solid transparent;
}

.tab:hover {
  background: #e0e0e0;
  color: #333;
}

.tab.active {
  background: white;
  color: #667eea;
  border-bottom-color: #667eea;
}

.tab .count {
  font-size: 12px;
  color: #999;
  margin-left: 4px;
}

/* Search */
.search-section {
  padding: 16px;
  border-bottom: 1px solid #e0e0e0;
}

.search-input {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s;
}

.search-input:focus {
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

/* Placeholder List */
.placeholder-list {
  flex: 1;
  overflow-y: auto;
  padding: 8px;
}

.placeholder-item {
  padding: 12px;
  margin-bottom: 8px;
  border: 1px solid #e0e0e0;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
  background: white;
}

.placeholder-item:hover {
  border-color: #667eea;
  background: #f8f9ff;
  transform: translateX(4px);
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.1);
}

.item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.placeholder-code {
  font-family: 'Courier New', monospace;
  font-size: 13px;
  color: #667eea;
  background: #f0f0f0;
  padding: 4px 8px;
  border-radius: 4px;
  font-weight: 600;
}

.required-badge {
  font-size: 11px;
  background: #ff4444;
  color: white;
  padding: 2px 6px;
  border-radius: 3px;
  font-weight: 600;
}

.item-meta {
  display: flex;
  gap: 12px;
  align-items: center;
  font-size: 12px;
  color: #666;
}

.type-badge {
  padding: 2px 8px;
  border-radius: 3px;
  font-weight: 600;
  font-size: 11px;
  text-transform: uppercase;
}

.type-string { background: #e3f2fd; color: #1976d2; }
.type-number { background: #fff3e0; color: #f57c00; }
.type-date { background: #f3e5f5; color: #7b1fa2; }
.type-boolean { background: #e8f5e9; color: #388e3c; }

.example {
  flex: 1;
  color: #999;
}

.example strong {
  color: #333;
}

/* Footer */
.selector-footer {
  padding: 12px 16px;
  border-top: 1px solid #e0e0e0;
  background: #f9f9f9;
  text-align: center;
}

.selector-footer small {
  color: #666;
}

/* Empty State */
.empty-state {
  text-align: center;
  padding: 40px 20px;
  color: #999;
}

/* Loading & Error */
.loading, .error {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 200px;
  padding: 20px;
}

.spinner {
  font-size: 18px;
  color: #667eea;
}

.error-message {
  text-align: center;
  color: #f44336;
}

.retry-btn {
  margin-top: 12px;
  padding: 8px 16px;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
}

.retry-btn:hover {
  background: #5568d3;
}
```

---

## ?? Step 5: Integrate vào Template Editor

C?p nh?t component `TemplateEditor.tsx`:

```typescript
import React, { useState, useRef } from 'react';
import PlaceholderSelector from './PlaceholderSelector';
import { templateService } from '@/services/templateService';

interface Props {
  templateType?: string;
  initialContent?: string;
  onSave?: (content: string) => void;
}

const TemplateEditor: React.FC<Props> = ({
  templateType = 'contract',
  initialContent = '',
  onSave
}) => {
  const [htmlContent, setHtmlContent] = useState(initialContent);
  const [showPlaceholderSelector, setShowPlaceholderSelector] = useState(false);
  const [validationErrors, setValidationErrors] = useState<string[]>([]);
  const editorRef = useRef<HTMLTextAreaElement>(null);

  // Insert placeholder vào v? trí con tr?
  const handleInsertPlaceholder = (placeholder: string) => {
    if (editorRef.current) {
      const { selectionStart, selectionEnd } = editorRef.current;
      const before = htmlContent.substring(0, selectionStart);
      const after = htmlContent.substring(selectionEnd);
      
      const newContent = before + placeholder + after;
      setHtmlContent(newContent);
      
      // Move cursor after inserted placeholder
      setTimeout(() => {
        if (editorRef.current) {
          const newPosition = selectionStart + placeholder.length;
          editorRef.current.setSelectionRange(newPosition, newPosition);
          editorRef.current.focus();
        }
      }, 0);
    }
    
    setShowPlaceholderSelector(false);
  };

  // Validate template tr??c khi l?u
  const handleValidate = async () => {
    try {
      // 1. Extract placeholders
      const placeholders = await templateService.extractPlaceholders(htmlContent);

      // 2. Validate v?i schema
      const validation = await templateService.validatePlaceholderSchema(
        placeholders,
        templateType
      );

      if (!validation.isValid && validation.invalidPlaceholders) {
        setValidationErrors(validation.invalidPlaceholders);
        alert(`? Template có ${validation.invalidPlaceholders.length} placeholders không h?p l?:\n${validation.invalidPlaceholders.join('\n')}`);
        return false;
      }

      setValidationErrors([]);
      alert('? Template h?p l?!');
      return true;
    } catch (error: any) {
      console.error('Validation error:', error);
      alert(`? L?i khi validate: ${error.message}`);
      return false;
    }
  };

  // Save template
  const handleSave = async () => {
    const isValid = await handleValidate();
    if (isValid && onSave) {
      onSave(htmlContent);
    }
  };

  return (
    <div className="template-editor">
      {/* Toolbar */}
      <div className="editor-toolbar">
        <h2>Template Editor</h2>
        <div className="toolbar-actions">
          <button 
            className="btn-insert-placeholder"
            onClick={() => setShowPlaceholderSelector(true)}
          >
            ?? Chèn Bi?n ??ng
          </button>
          <button className="btn-validate" onClick={handleValidate}>
            ? Validate
          </button>
          <button className="btn-save" onClick={handleSave}>
            ?? L?u
          </button>
        </div>
      </div>

      {/* Editor */}
      <div className="editor-body">
        <textarea
          ref={editorRef}
          value={htmlContent}
          onChange={(e) => setHtmlContent(e.target.value)}
          placeholder="Nh?p HTML template c?a b?n...&#10;&#10;S? d?ng placeholders: {{Entity.Property}}&#10;Ví d?: {{Contract.NumberContract}}, {{Customer.Name}}"
          className="html-editor"
        />

        {/* Validation Errors */}
        {validationErrors.length > 0 && (
          <div className="validation-errors">
            <h4>? Placeholders không h?p l?:</h4>
            <ul>
              {validationErrors.map((err, idx) => (
                <li key={idx}>{err}</li>
              ))}
            </ul>
          </div>
        )}
      </div>

      {/* Placeholder Selector Modal */}
      {showPlaceholderSelector && (
        <div className="modal-overlay">
          <div className="modal-backdrop" onClick={() => setShowPlaceholderSelector(false)} />
          <div className="modal-content">
            <PlaceholderSelector
              templateType={templateType}
              onInsertPlaceholder={handleInsertPlaceholder}
              onClose={() => setShowPlaceholderSelector(false)}
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

## ?? Step 6: CSS cho Template Editor

T?o/c?p nh?t `TemplateEditor.css`:

```css
.template-editor {
  display: flex;
  flex-direction: column;
  height: 100vh;
  background: #f5f5f5;
}

/* Toolbar */
.editor-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  background: white;
  border-bottom: 1px solid #e0e0e0;
}

.editor-toolbar h2 {
  margin: 0;
  font-size: 20px;
  font-weight: 600;
}

.toolbar-actions {
  display: flex;
  gap: 12px;
}

.toolbar-actions button {
  padding: 10px 20px;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-insert-placeholder {
  background: #667eea;
  color: white;
}

.btn-insert-placeholder:hover {
  background: #5568d3;
  transform: translateY(-1px);
  box-shadow: 0 4px 8px rgba(102, 126, 234, 0.3);
}

.btn-validate {
  background: #4caf50;
  color: white;
}

.btn-validate:hover {
  background: #45a049;
}

.btn-save {
  background: #2196f3;
  color: white;
}

.btn-save:hover {
  background: #0b7dda;
}

/* Editor Body */
.editor-body {
  flex: 1;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  overflow: hidden;
}

.html-editor {
  flex: 1;
  padding: 16px;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-family: 'Courier New', monospace;
  font-size: 14px;
  line-height: 1.6;
  resize: none;
  outline: none;
  background: white;
}

.html-editor:focus {
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

/* Validation Errors */
.validation-errors {
  background: #ffebee;
  border: 1px solid #ef5350;
  border-radius: 6px;
  padding: 16px;
  max-height: 200px;
  overflow-y: auto;
}

.validation-errors h4 {
  margin: 0 0 12px 0;
  color: #d32f2f;
  font-size: 16px;
}

.validation-errors ul {
  margin: 0;
  padding-left: 20px;
  color: #c62828;
}

.validation-errors li {
  margin-bottom: 4px;
  font-family: 'Courier New', monospace;
  font-size: 13px;
}

/* Modal */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
}

.modal-backdrop {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
}

.modal-content {
  position: relative;
  z-index: 1001;
  animation: slideIn 0.3s ease-out;
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
```

---

## ?? Step 7: Usage Example

```typescript
// pages/templates/create.tsx ho?c app/templates/create/page.tsx

import TemplateEditor from '@/components/TemplateEditor';
import { templateService } from '@/services/templateService';

export default function CreateTemplatePage() {
  const handleSave = async (htmlContent: string) => {
    try {
      await templateService.createTemplate({
        name: 'My Template',
        templateType: 'contract',
        code: 'MY_TEMPLATE_001',
        htmlContent: htmlContent,
        isActive: true,
        isDefault: false
      });

      alert('? Template ?ã ???c l?u!');
      // Redirect ho?c refresh
    } catch (error: any) {
      alert(`? L?i: ${error.message}`);
    }
  };

  return (
    <div>
      <TemplateEditor
        templateType="contract"
        onSave={handleSave}
      />
    </div>
  );
}
```

---

## ?? Step 8: Render Template v?i Data

Khi c?n render template (ví d?: generate contract PDF):

```typescript
// utils/contractGenerator.ts

import { templateService } from '@/services/templateService';

export async function generateContractHtml(contractId: number): Promise<string> {
  try {
    // 1. Fetch contract data t? API
    const contract = await fetch(`/api/contracts/${contractId}`).then(r => r.json());

    // 2. Chu?n b? structured data
    const data = {
      Contract: {
        NumberContract: contract.numberContract,
        Status: contract.status,
        TotalAmount: contract.totalAmount,
        SubTotal: contract.subTotal,
        TaxAmount: contract.taxAmount,
        Expiration: contract.expiration,
        Notes: contract.notes
      },
      Customer: {
        Name: contract.customer.name,
        Email: contract.customer.email,
        PhoneNumber: contract.customer.phoneNumber,
        CompanyName: contract.customer.companyName,
        CompanyAddress: contract.customer.companyAddress,
        TaxCode: contract.customer.taxCode,
        RepresentativeName: contract.customer.representativeName,
        RepresentativeEmail: contract.customer.representativeEmail,
        RepresentativePhone: contract.customer.representativePhone
      },
      SaleOrder: {
        Title: contract.saleOrder.title,
        Value: contract.saleOrder.value,
        Status: contract.saleOrder.status
      },
      User: {
        FullName: contract.user.fullName,
        Email: contract.user.email,
        Department: contract.user.department
      }
    };

    // 3. Render template
    const html = await templateService.renderTemplateWithObjectByCode(
      'CONTRACT_DEFAULT',
      data
    );

    return html;
  } catch (error: any) {
    console.error('Error generating contract:', error);
    throw new Error(`Không th? t?o h?p ??ng: ${error.message}`);
  }
}
```

---

## ? Checklist Implementation

### Setup
- [ ] Install `axios`
- [ ] T?o `templateService.ts`
- [ ] Configure API_URL trong `.env.local`

### Components
- [ ] T?o `PlaceholderSelector.tsx`
- [ ] T?o `PlaceholderSelector.css`
- [ ] Update/t?o `TemplateEditor.tsx`
- [ ] T?o `TemplateEditor.css`

### Integration
- [ ] Test PlaceholderSelector standalone
- [ ] Test insert placeholder vào editor
- [ ] Test validation
- [ ] Test save template
- [ ] Test render template v?i data

### Testing
- [ ] Test v?i template type khác nhau (contract, quote, invoice)
- [ ] Test search trong PlaceholderSelector
- [ ] Test validation v?i invalid placeholders
- [ ] Test render v?i real contract data

---

## ?? Best Practices

### 1. ? Dùng Nested Syntax
```html
<!-- ? Good - Rõ ràng -->
{{Contract.NumberContract}}
{{Customer.CompanyName}}

<!-- ?? OK nh?ng không khuy?n khích -->
{{NumberContract}}
{{CompanyName}}
```

### 2. ? Validate Tr??c Khi Save
```typescript
const isValid = await handleValidate();
if (!isValid) {
  return; // Không cho l?u n?u invalid
}
```

### 3. ? Hi?n th? Type Badge
User c?n bi?t field nào là date, number ?? format ?úng khi nh?p sample data.

### 4. ? Structured Data Khi Render
```typescript
// ? Good
const data = {
  Contract: { ... },
  Customer: { ... }
};

// ? Không khuy?n khích (dù v?n work)
const data = {
  "Contract.NumberContract": "123",
  "Customer.Name": "..."
};
```

---

## ?? Troubleshooting

### Issue: "Cannot find module '@/services/templateService'"
**Solution:** ??m b?o b?n ?ã config path alias trong `tsconfig.json`:
```json
{
  "compilerOptions": {
    "paths": {
      "@/*": ["./src/*"]
    }
  }
}
```

### Issue: CORS Error
**Solution:** Backend ?ã config CORS cho `http://localhost:5173`. N?u b?n dùng port khác, update trong `Program.cs`:
```csharp
policy.WithOrigins("http://localhost:3000") // Your port
```

### Issue: 401 Unauthorized
**Solution:** ??m b?o token ???c l?u trong localStorage và ???c g?i trong header:
```typescript
localStorage.setItem('token', 'your-jwt-token');
```

---

## ?? Tài Li?u Tham Kh?o

- [PLACEHOLDER_SCHEMA_DOCUMENTATION.md](./PLACEHOLDER_SCHEMA_DOCUMENTATION.md) - Full API documentation
- [README_TEMPLATE_SYSTEM.md](./README_TEMPLATE_SYSTEM.md) - System overview
- [QUICK_REFERENCE_PLACEHOLDER_SCHEMA.md](./QUICK_REFERENCE_PLACEHOLDER_SCHEMA.md) - Quick reference

---

## ?? Next Steps

1. ? **Setup** - Install dependencies và t?o service
2. ? **Components** - T?o PlaceholderSelector và integrate vào editor
3. ?? **Testing** - Test v?i real data
4. ?? **Enhancement** - Thêm features nh?:
   - Monaco Editor integration (syntax highlighting)
   - Live preview khi gõ
   - Template snippets
   - Undo/Redo

---

**Chúc b?n code vui v?! ??**

*Last updated: 2024-12-31*
