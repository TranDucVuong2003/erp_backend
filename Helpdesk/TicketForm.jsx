import React, { useState, useRef, useEffect } from 'react'
import { CKEditor } from '@ckeditor/ckeditor5-react';
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';
import {
  PlusIcon,
  MagnifyingGlassIcon as SearchIcon,
  FunnelIcon as FilterIcon,
  ClockIcon,
  ExclamationTriangleIcon,
  CheckCircleIcon,
  XCircleIcon,
  ChatBubbleLeftRightIcon,
  UserIcon,
  CalendarIcon,
  TagIcon,
  PaperClipIcon,
  ChevronDownIcon,
  XMarkIcon,
  EyeIcon,
  StarIcon
} from '@heroicons/react/24/outline';
import { StarIcon as StarIconSolid } from '@heroicons/react/24/solid';

const TicketForm = ({ ticket, onSubmit }) => {
  const [formData, setFormData] = useState({
    title: ticket?.title || '',
    description: ticket?.description || '',
    customer: ticket?.customer || { id: '', name: '', email: '', phone: '' },
    priority: ticket?.priority || 'medium',
    category: ticket?.category || 'technical',
    assignedTo: ticket?.assignedTo || { id: '', name: '', email: '' },
    dueDate: ticket?.dueDate || '',
    tags: ticket?.tags || [],
    status: ticket?.status || 'new',
    stars: ticket?.stars || 1
  });

  // Customer search states
  const [customerSearchTerm, setCustomerSearchTerm] = useState('');
  const [isCustomerDropdownOpen, setIsCustomerDropdownOpen] = useState(false);
  const [isCustomerPopupOpen, setIsCustomerPopupOpen] = useState(false);
  const customerDropdownRef = useRef(null);
  const customerInputRef = useRef(null);

  const customers = [
    { id: 1, name: 'Nguyen Van A', email: 'nguyenvana@email.com', phone: '0901234567', company: 'ABC Corp' },
    { id: 2, name: 'Tran Thi B', email: 'tranthib@email.com', phone: '0987654321', company: 'XYZ Ltd' },
    { id: 3, name: 'Le Van C', email: 'levanc@email.com', phone: '0912345678', company: 'DEF Inc' },
    { id: 4, name: 'Pham Thi D', email: 'phamthid@email.com', phone: '0923456789', company: 'GHI Co' },
    { id: 5, name: 'Hoang Van E', email: 'hoangvane@email.com', phone: '0934567890', company: 'JKL Ltd' },
    { id: 6, name: 'Nguyen Thi F', email: 'nguyenthif@email.com', phone: '0945678901', company: 'MNO Corp' },
    { id: 7, name: 'Tran Van G', email: 'tranvang@email.com', phone: '0956789012', company: 'PQR Inc' },
    { id: 8, name: 'Le Thi H', email: 'lethih@email.com', phone: '0967890123', company: 'STU Co' },
    { id: 9, name: 'Pham Van I', email: 'phamvani@email.com', phone: '0978901234', company: 'VWX Ltd' },
    { id: 10, name: 'Hoang Thi J', email: 'hoangthij@email.com', phone: '0989012345', company: 'YZ Corp' }
  ];

  const consultants = [
    { id: 1, name: 'John Smith', role: 'Senior Consultant' },
    { id: 2, name: 'Jane Doe', role: 'Technical Consultant' },
    { id: 3, name: 'Mike Johnson', role: 'Support Consultant' }
  ];

  const helpdeskAgents = [
    { id: 1, name: 'Trần Thị B', email: 'tranthib@company.com' },
    { id: 2, name: 'Phạm Văn D', email: 'phamvand@company.com' },
    { id: 3, name: 'Vũ Văn F', email: 'vuvanf@company.com' }
  ];

  const categories = [
    { value: 'technical', label: 'Technical Issue' },
    { value: 'bug', label: 'Bug Report' },
    { value: 'feature_request', label: 'Feature Request' },
    { value: 'account', label: 'Account Issue' },
    { value: 'billing', label: 'Billing Issue' },
    { value: 'general', label: 'General Inquiry' }
  ];

  const priorities = [
    { value: 'low', label: 'Low', color: 'text-green-600' },
    { value: 'medium', label: 'Medium', color: 'text-yellow-600' },
    { value: 'high', label: 'High', color: 'text-orange-600' },
    { value: 'critical', label: 'Critical', color: 'text-red-600' }
  ];

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleDescriptionChange = (event, editor) => {
    const data = editor.getData();
    setFormData(prev => ({
      ...prev,
      description: data
    }));
  };

  const handleCustomerChange = (e) => {
    const selectedCustomer = customers.find(c => c.id === parseInt(e.target.value));
    setFormData(prev => ({
      ...prev,
      customer: selectedCustomer || { id: '', name: '', email: '', phone: '' }
    }));
  };

  const handleAssignedToChange = (e) => {
    const selectedAgent = helpdeskAgents.find(a => a.id === parseInt(e.target.value));
    setFormData(prev => ({
      ...prev,
      assignedTo: selectedAgent || { id: '', name: '', email: '' }
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    onSubmit(formData);
  };

  const handleStarClick = (starNumber) => {
    setFormData(prev => ({
      ...prev,
      stars: starNumber
    }));
  };

  const renderStars = () => {
    const stars = [];
    for (let i = 1; i <= 5; i++) {
      stars.push(
        <button
          key={i}
          type="button"
          onClick={() => handleStarClick(i)}
          className={`p-1 transition-colors ${
            i <= formData.stars 
              ? 'text-yellow-500 hover:text-yellow-600' 
              : 'text-gray-300 hover:text-gray-400'
          }`}
        >
          {i <= formData.stars ? (
            <StarIconSolid className="h-6 w-6" />
          ) : (
            <StarIcon className="h-6 w-6" />
          )}
        </button>
      );
    }
    return stars;
  };

  // Filter customers based on search term
  const filteredCustomers = customers.filter(customer =>
    customer.name.toLowerCase().includes(customerSearchTerm.toLowerCase()) ||
    customer.email.toLowerCase().includes(customerSearchTerm.toLowerCase()) ||
    customer.phone.includes(customerSearchTerm) ||
    customer.company.toLowerCase().includes(customerSearchTerm.toLowerCase())
  );

  // Close dropdown when clicking outside
  useEffect(() => {
    const handleClickOutside = (event) => {
      if (customerDropdownRef.current && !customerDropdownRef.current.contains(event.target)) {
        setIsCustomerDropdownOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  const handleCustomerSelect = (customer) => {
    setFormData(prev => ({
      ...prev,
      customer: customer
    }));
    setCustomerSearchTerm(customer.name);
    setIsCustomerDropdownOpen(false);
    setIsCustomerPopupOpen(false);
  };

  const handleCustomerInputFocus = () => {
    setIsCustomerDropdownOpen(true);
    if (!customerSearchTerm && formData.customer.name) {
      setCustomerSearchTerm('');
    }
  };

  const handleCustomerSearchChange = (e) => {
    setCustomerSearchTerm(e.target.value);
    setIsCustomerDropdownOpen(true);
  };

  // Initialize customer search term when form opens
  useEffect(() => {
    if (formData.customer.name) {
      setCustomerSearchTerm(formData.customer.name);
    }
  }, [formData.customer.name]);

  return (
    <>
      <div className="max-w-5xl mx-auto">
        <div className="bg-white rounded-lg shadow-lg border border-gray-200">
          {/* Header Section */}
          <div className="bg-gradient-to-r from-indigo-400 to-indigo-600 rounded-t-lg px-6 py-4">
            <h2 className="text-2xl font-bold text-white mb-1">
              {ticket ? 'Edit Ticket' : 'Create New Ticket'}
            </h2>
            <p className="text-indigo-100 text-sm">
              {ticket ? 'Update the ticket information below' : 'Fill out the form below to create a new support ticket'}
            </p>
          </div>

          {/* Form Content */}
          <div className="p-6">

          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Ticket Title */}
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">
                Ticket Title *
              </label>
              <input
                type="text"
                name="title"
                value={formData.title}
                onChange={handleInputChange}
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors"
                placeholder="Enter a clear, descriptive ticket title"
                required
              />
            </div>

            {/* Customer Selection with Search */}
            <div className="relative" ref={customerDropdownRef}>
              <label className="block text-sm font-semibold text-gray-700 mb-2">
                Customer *
              </label>
              <div className="relative">
                <input
                  ref={customerInputRef}
                  type="text"
                  value={customerSearchTerm}
                  onChange={handleCustomerSearchChange}
                  onFocus={handleCustomerInputFocus}
                  className="w-full px-4 py-3 pr-20 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors"
                  placeholder="Search customers by name, email, or company..."
                  required
                />
                <div className="absolute right-2 top-1/2 transform -translate-y-1/2 flex items-center space-x-1">
                  <button
                    type="button"
                    onClick={() => setIsCustomerPopupOpen(true)}
                    className="p-1 text-gray-400 hover:text-indigo-600 transition-colors"
                    title="Advanced Search"
                  >
                    <SearchIcon className="h-4 w-4" />
                  </button>
                  <ChevronDownIcon 
                    className={`h-4 w-4 text-gray-400 transition-transform ${isCustomerDropdownOpen ? 'rotate-180' : ''}`} 
                  />
                </div>

                {/* Dropdown List */}
                {isCustomerDropdownOpen && (
                  <div className="absolute z-10 w-full mt-1 bg-white border border-gray-300 rounded-md shadow-lg max-h-60 overflow-y-auto">
                    {filteredCustomers.length > 0 ? (
                      filteredCustomers.map(customer => (
                        <div
                          key={customer.id}
                          onClick={() => handleCustomerSelect(customer)}
                          className="px-3 py-2 hover:bg-indigo-50 cursor-pointer border-b border-gray-100 last:border-b-0"
                        >
                          <div className="flex items-center justify-between">
                            <div>
                              <div className="font-medium text-gray-900">{customer.name}</div>
                              <div className="text-sm text-gray-600">{customer.email}</div>
                              <div className="text-xs text-gray-500">{customer.company} • {customer.phone}</div>
                            </div>
                            <UserIcon className="h-4 w-4 text-gray-400" />
                          </div>
                        </div>
                      ))
                    ) : (
                      <div className="px-3 py-2 text-gray-500 text-center">
                        No customers found
                      </div>
                    )}
                  </div>
                )}
              </div>
            </div>

            {/* Consultant Selection */}
            {/* Priority Selection */}
            {/* <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Priority *
              </label>
              <div className="relative">
                <select
                  name="priority"
                  value={formData.priority}
                  onChange={handleInputChange}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 appearance-none bg-white"
                  required
                >
                  {priorities.map(priority => (
                    <option key={priority.value} value={priority.value}>
                      {priority.label}
                    </option>
                  ))}
                </select>
                <ChevronDownIcon className="absolute right-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400 pointer-events-none" />
              </div>
            </div> */}

            {/* Stars Rating */}
            <div className="bg-gray-50 p-4 rounded-lg border">
              <label className="block text-sm font-semibold text-gray-700 mb-3">
                Mức độ khẩn cấp (Sao) *
              </label>
              <div className="flex items-center space-x-2">
                <div className="flex items-center space-x-1">
                  {renderStars()}
                </div>
                <span className="ml-4 text-sm font-medium text-gray-700 bg-white px-3 py-1 rounded-full border">
                  {formData.stars} sao - {formData.stars === 1 ? 'Bình thường' : 
                   formData.stars === 2 ? 'Quan trọng' : 
                   formData.stars === 3 ? 'Khẩn cấp' : 
                   formData.stars === 4 ? 'Rất khẩn cấp' : 'Cực kỳ khẩn cấp'}
                </span>
              </div>
            </div>

            {/* Category Selection */}
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">
                Category *
              </label>
              <div className="relative">
                <select
                  name="category"
                  value={formData.category}
                  onChange={handleInputChange}
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 appearance-none bg-white transition-colors"
                  required
                >
                  {categories.map(category => (
                    <option key={category.value} value={category.value}>
                      {category.label}
                    </option>
                  ))}
                </select>
                <ChevronDownIcon className="absolute right-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400 pointer-events-none" />
              </div>
            </div>

            {/* Assigned To */}
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">
                Assigned To
              </label>
              <div className="relative">
                <select
                  value={formData.assignedTo.id}
                  onChange={handleAssignedToChange}
                  className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 appearance-none bg-white transition-colors"
                >
                  <option value="">Select an agent to assign</option>
                  {helpdeskAgents.map(agent => (
                    <option key={agent.id} value={agent.id}>
                      {agent.name} - {agent.email}
                </option>
              ))}
            </select>
            <ChevronDownIcon className="absolute right-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400 pointer-events-none" />
          </div>
        </div>

            {/* Description with CKEditor */}
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">
                Description *
              </label>
              <div className="border border-gray-300 rounded-md overflow-hidden focus-within:ring-2 focus-within:ring-indigo-500 focus-within:border-indigo-500">
                <CKEditor
                  editor={ClassicEditor}
                  data={formData.description || ''}
                  onChange={handleDescriptionChange}
                  config={{
                    toolbar: [
                      'heading', '|',
                      'bold', 'italic', 'underline', 'strikethrough', '|',
                      'bulletedList', 'numberedList', '|',
                      'outdent', 'indent', '|',
                      'link', 'blockQuote', '|',
                      'insertTable', 'tableColumn', 'tableRow', 'mergeTableCells', '|',
                      'undo', 'redo'
                    ],
                    heading: {
                      options: [
                        { model: 'paragraph', title: 'Paragraph', class: 'ck-heading_paragraph' },
                        { model: 'heading1', view: 'h1', title: 'Heading 1', class: 'ck-heading_heading1' },
                        { model: 'heading2', view: 'h2', title: 'Heading 2', class: 'ck-heading_heading2' },
                        { model: 'heading3', view: 'h3', title: 'Heading 3', class: 'ck-heading_heading3' }
                      ]
                    },
                    placeholder: 'Describe the issue or request in detail...',
                    height: 350,
                    removePlugins: ['ImageUpload', 'EasyImage', 'ImageInsert', 'MediaEmbed'],
                    table: {
                      contentToolbar: ['tableColumn', 'tableRow', 'mergeTableCells']
                    }
                  }}
                  onReady={editor => {
                    // Set minimum height for the editing area
                    editor.editing.view.change(writer => {
                      writer.setStyle('min-height', '280px', editor.editing.view.document.getRoot());
                    });
                    console.log('CKEditor is ready to use!', editor);
                  }}
                  onError={(error, { willEditorRestart }) => {
                    if (willEditorRestart) {
                      console.log('CKEditor will restart');
                    } else {
                      console.error('CKEditor error:', error);
                    }
                  }}
                />
              </div>
              <p className="mt-1 text-xs text-gray-500">
                Use rich text formatting to provide detailed information about the ticket
              </p>
            </div>

            {/* Due Date */}
            <div>
              <label className="block text-sm font-semibold text-gray-700 mb-2">
                Due Date
              </label>
              <input
                type="datetime-local"
                name="dueDate"
                value={formData.dueDate ? formData.dueDate.slice(0, 16) : ''}
                onChange={handleInputChange}
                className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition-colors"
              />
              <p className="mt-1 text-xs text-gray-500">
                Set a deadline for when this ticket should be resolved
              </p>
            </div>

            {/* Form Actions */}
            <div className="flex space-x-3 pt-6 border-t border-gray-200 mt-6">
              <button
                type="submit"
                className="w-full bg-indigo-600 text-white py-3 px-6 rounded-md hover:bg-indigo-700 focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 transition duration-200 font-medium shadow-lg"
              >
                {ticket ? 'Update Ticket' : 'Create Ticket'}
              </button>
            </div>
          </form>
          </div>
        </div>
      </div>

      {/* Customer Search Popup */}
      {isCustomerPopupOpen && (
        <CustomerSearchPopup
          isOpen={isCustomerPopupOpen}
          onClose={() => setIsCustomerPopupOpen(false)}
          customers={customers}
          onSelectCustomer={handleCustomerSelect}
          selectedCustomer={formData.customer}
        />
      )}
    </>
  );
};

// Customer Search Popup Component
const CustomerSearchPopup = ({ isOpen, onClose, customers, onSelectCustomer, selectedCustomer }) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedFilters, setSelectedFilters] = useState({
    company: '',
    hasPhone: false,
    hasEmail: true
  });

  const filteredCustomers = customers.filter(customer => {
    const matchesSearch = 
      customer.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      customer.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
      customer.phone.includes(searchTerm) ||
      customer.company.toLowerCase().includes(searchTerm.toLowerCase());

    const matchesCompany = !selectedFilters.company || 
      customer.company.toLowerCase().includes(selectedFilters.company.toLowerCase());

    const matchesPhone = !selectedFilters.hasPhone || customer.phone;
    const matchesEmail = !selectedFilters.hasEmail || customer.email;

    return matchesSearch && matchesCompany && matchesPhone && matchesEmail;
  });

  const companies = [...new Set(customers.map(c => c.company))].sort();

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0  bg-opacity-50 flex items-center justify-center z-[60]" style={{ backgroundColor: "rgba(0,0,0,0.5)" }}>
      <div className="bg-white rounded-lg shadow-xl max-w-4xl w-full mx-4 max-h-[80vh] overflow-hidden">
        <div className="bg-white rounded-lg">
          {/* Header */}
          <div className="flex items-center justify-between p-6 border-b border-gray-200">
            <h3 className="text-lg font-semibold text-gray-900">Search Customers</h3>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600"
              type="button"
            >
              <XMarkIcon className="h-6 w-6" />
            </button>
          </div>

          {/* Search and Filters */}
          <div className="p-6 border-b border-gray-200 bg-gray-50">
            <div className="flex flex-col md:flex-row gap-4">
              {/* Search Input */}
              <div className="flex-1">
                <div className="relative">
                  <SearchIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400" />
                  <input
                    type="text"
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
                    placeholder="Search by name, email, phone, or company..."
                  />
                </div>
              </div>

              {/* Company Filter */}
              <div className="w-48">
                <select
                  value={selectedFilters.company}
                  onChange={(e) => setSelectedFilters(prev => ({ ...prev, company: e.target.value }))
                  }
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
                >
                  <option value="">All Companies</option>
                  {companies.map(company => (
                    <option key={company} value={company}>{company}</option>
                  ))}
                </select>
              </div>
            </div>
          </div>

          {/* Results */}
          <div className="overflow-y-auto max-h-96">
            {filteredCustomers.length > 0 ? (
              <div className="divide-y divide-gray-100">
                {filteredCustomers.map(customer => (
                  <div
                    key={customer.id}
                    onClick={() => onSelectCustomer(customer)}
                    className={`p-4 hover:bg-indigo-50 cursor-pointer transition-colors ${
                      selectedCustomer?.id === customer.id ? 'bg-indigo-100 border-l-4 border-indigo-500' : ''
                    }`}
                  >
                    <div className="flex items-center justify-between">
                      <div className="flex-1">
                        <div className="flex items-center space-x-3">
                          <div className="flex-shrink-0">
                            <div className="w-10 h-10 bg-indigo-100 rounded-full flex items-center justify-center">
                              <UserIcon className="h-5 w-5 text-indigo-600" />
                            </div>
                          </div>
                          <div className="flex-1 min-w-0">
                            <div className="flex items-center space-x-2">
                              <h4 className="text-sm font-medium text-gray-900 truncate">
                                {customer.name}
                              </h4>
                              {selectedCustomer?.id === customer.id && (
                                <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium bg-indigo-100 text-indigo-800">
                                  Selected
                                </span>
                              )}
                            </div>
                            <div className="flex items-center space-x-4 mt-1">
                              <span className="text-sm text-gray-600 truncate">{customer.email}</span>
                              <span className="text-sm text-gray-500">{customer.phone}</span>
                            </div>
                            <div className="text-xs text-gray-500 mt-1">{customer.company}</div>
                          </div>
                        </div>
                      </div>
                      <div className="flex-shrink-0">
                        <ChevronDownIcon className="h-5 w-5 text-gray-400 rotate-[-90deg]" />
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="p-8 text-center">
                <UserIcon className="mx-auto h-12 w-12 text-gray-400" />
                <h3 className="mt-2 text-sm font-medium text-gray-900">No customers found</h3>
                <p className="mt-1 text-sm text-gray-500">Try adjusting your search criteria</p>
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="p-4 border-t border-gray-200 bg-gray-50 flex justify-between items-center">
            <div className="text-sm text-gray-600">
              {filteredCustomers.length} customer{filteredCustomers.length !== 1 ? 's' : ''} found
            </div>
            <button
              onClick={onClose}
              className="px-4 py-2 bg-gray-300 text-gray-700 rounded-md hover:bg-gray-400 transition duration-200"
            >
              Close
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default TicketForm;
