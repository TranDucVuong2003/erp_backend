// ============================================================
// PlaceholderSelector.tsx
// Component hi?n th? danh sách placeholders có nhóm theo entity
// ============================================================

import React, { useState, useEffect } from 'react';
import axios from 'axios';

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

interface PlaceholderField {
  name: string;
  placeholder: string;
  type: string;
  description: string;
  isRequired: boolean;
  example: string;
}

interface Props {
  templateType: string; // 'contract', 'quote', 'invoice'...
  onInsertPlaceholder: (placeholder: string) => void;
  onClose?: () => void;
}

const PlaceholderSelector: React.FC<Props> = ({
  templateType,
  onInsertPlaceholder,
  onClose
}) => {
  const [placeholders, setPlaceholders] = useState<Record<string, PlaceholderField[]>>({});
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
      const token = localStorage.getItem('token');
      const response = await axios.get(
        `${API_URL}/api/DocumentTemplates/schema/placeholders`,
        {
          params: { templateType },
          headers: { Authorization: `Bearer ${token}` }
        }
      );

      const { data } = response.data;
      setPlaceholders(data);

      // Auto-select first entity
      const firstEntity = Object.keys(data)[0];
      setSelectedEntity(firstEntity);
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
          <button onClick={fetchPlaceholders}>Th? l?i</button>
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
          <button className="close-btn" onClick={onClose}>?</button>
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
            Không tìm th?y placeholder nào
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

      {/* Footer Info */}
      <div className="selector-footer">
        <small>
          ?? Click vào placeholder ?? chèn vào template
        </small>
      </div>
    </div>
  );
};

// ============================================================
// CSS Styles (PlaceholderSelector.module.css)
// ============================================================

const styles = `
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

.selector-footer {
  padding: 12px 16px;
  border-top: 1px solid #e0e0e0;
  background: #f9f9f9;
  text-align: center;
}

.selector-footer small {
  color: #666;
}

.empty-state {
  text-align: center;
  padding: 40px 20px;
  color: #999;
}

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

.error-message button {
  margin-top: 12px;
  padding: 8px 16px;
  background: #667eea;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
}

.error-message button:hover {
  background: #5568d3;
}
`;

// ============================================================
// Usage Example trong Template Editor
// ============================================================

const TemplateEditor: React.FC = () => {
  const [htmlContent, setHtmlContent] = useState('');
  const [showPlaceholderSelector, setShowPlaceholderSelector] = useState(false);
  const [templateType, setTemplateType] = useState('contract');
  const editorRef = React.useRef<HTMLTextAreaElement>(null);

  const handleInsertPlaceholder = (placeholder: string) => {
    if (editorRef.current) {
      const { selectionStart, selectionEnd } = editorRef.current;
      const before = htmlContent.substring(0, selectionStart);
      const after = htmlContent.substring(selectionEnd);
      
      setHtmlContent(before + placeholder + after);
      
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

  return (
    <div className="template-editor">
      <div className="editor-header">
        <h2>Template Editor</h2>
        <select 
          value={templateType} 
          onChange={(e) => setTemplateType(e.target.value)}
        >
          <option value="contract">H?p ??ng</option>
          <option value="quote">Báo giá</option>
          <option value="invoice">Hóa ??n</option>
          <option value="email">Email</option>
        </select>
      </div>

      <div className="editor-body">
        <textarea
          ref={editorRef}
          value={htmlContent}
          onChange={(e) => setHtmlContent(e.target.value)}
          placeholder="Nh?p HTML template..."
          className="html-editor"
        />

        <button 
          className="insert-placeholder-btn"
          onClick={() => setShowPlaceholderSelector(true)}
        >
          ?? Chèn Bi?n ??ng
        </button>
      </div>

      {showPlaceholderSelector && (
        <div className="placeholder-modal">
          <div className="modal-backdrop" onClick={() => setShowPlaceholderSelector(false)} />
          <PlaceholderSelector
            templateType={templateType}
            onInsertPlaceholder={handleInsertPlaceholder}
            onClose={() => setShowPlaceholderSelector(false)}
          />
        </div>
      )}
    </div>
  );
};

export default PlaceholderSelector;
export { TemplateEditor };

// ============================================================
// Service Example - templateService.ts
// ============================================================

export class TemplateService {
  private apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

  private getAuthHeaders() {
    const token = localStorage.getItem('token');
    return {
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json'
    };
  }

  // ? L?y available placeholders theo template type
  async getAvailablePlaceholders(templateType: string) {
    const response = await axios.get(
      `${this.apiUrl}/api/DocumentTemplates/schema/placeholders`,
      {
        params: { templateType },
        headers: this.getAuthHeaders()
      }
    );
    return response.data;
  }

  // ? L?y placeholders c?a m?t entity
  async getPlaceholdersForEntity(entityName: string) {
    const response = await axios.get(
      `${this.apiUrl}/api/DocumentTemplates/schema/placeholders/${entityName}`,
      { headers: this.getAuthHeaders() }
    );
    return response.data;
  }

  // ? Validate placeholders
  async validatePlaceholders(placeholders: string[], templateType: string) {
    const response = await axios.post(
      `${this.apiUrl}/api/DocumentTemplates/schema/validate-placeholders`,
      { placeholders, templateType },
      { headers: this.getAuthHeaders() }
    );
    return response.data;
  }

  // ? Render v?i object data
  async renderTemplateWithObject(templateId: number, data: any) {
    const response = await axios.post(
      `${this.apiUrl}/api/DocumentTemplates/render-with-object/${templateId}`,
      data,
      {
        headers: this.getAuthHeaders(),
        responseType: 'text'
      }
    );
    return response.data;
  }

  // ? Render by code v?i object
  async renderTemplateWithObjectByCode(templateCode: string, data: any) {
    const response = await axios.post(
      `${this.apiUrl}/api/DocumentTemplates/render-with-object-by-code/${templateCode}`,
      data,
      {
        headers: this.getAuthHeaders(),
        responseType: 'text'
      }
    );
    return response.data;
  }
}

export const templateService = new TemplateService();
