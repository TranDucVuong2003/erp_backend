import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
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
  ArrowLeftIcon
} from '@heroicons/react/24/outline';

const TicketEditPage = () => {
  const { ticketId } = useParams();
  const navigate = useNavigate();
  
  // Mock data - trong thực tế sẽ fetch từ API hoặc context
  const [tickets] = useState([
    {
      id: 'TCK-001',
      title: 'Không thể đăng nhập vào hệ thống',
      description: 'Khách hàng báo cáo không thể đăng nhập sau khi đổi mật khẩu',
      customer: {
        id: 1,
        name: 'Nguyễn Văn A',
        email: 'nguyenvana@email.com',
        phone: '0901234567'
      },
      priority: 'high',
      status: 'open',
      category: 'technical',
      assignedTo: {
        id: 1,
        name: 'Trần Thị B',
        email: 'tranthib@company.com'
      },
      createdAt: '2025-10-06T09:00:00Z',
      updatedAt: '2025-10-06T14:30:00Z',
      dueDate: '2025-10-08T17:00:00Z',
      slaBreached: false,
      tags: ['login', 'password', 'urgent'],
      attachments: [],
      comments: [
        {
          id: 1,
          author: 'Trần Thị B',
          content: 'Đã liên hệ khách hàng và hướng dẫn reset mật khẩu',
          createdAt: '2025-10-06T14:30:00Z',
          type: 'internal'
        }
      ]
    },
    {
      id: 'TCK-002', 
      title: 'Yêu cầu thêm tính năng báo cáo',
      description: 'Khách hàng muốn thêm tính năng xuất báo cáo theo tháng',
      customer: {
        id: 2,
        name: 'Lê Văn C',
        email: 'levanc@email.com', 
        phone: '0907654321'
      },
      priority: 'medium',
      status: 'in_progress',
      category: 'feature_request',
      assignedTo: {
        id: 2,
        name: 'Phạm Văn D',
        email: 'phamvand@company.com'
      },
      createdAt: '2025-10-05T14:00:00Z',
      updatedAt: '2025-10-06T16:00:00Z',
      dueDate: '2025-10-12T17:00:00Z',
      slaBreached: false,
      tags: ['feature', 'report', 'enhancement'],
      attachments: [],
      comments: []
    },{
      id: 'TCK-003',
      title: 'Lỗi hiển thị dữ liệu khách hàng',
      description: 'Danh sách khách hàng không load được, hiển thị màn hình trống',
      customer: {
        id: 3,
        name: 'Hoàng Thị E',
        email: 'hoangthie@email.com',
        phone: '0909876543'
      },
      priority: 'critical',
      status: 'escalated',
      category: 'bug',
      assignedTo: {
        id: 3,
        name: 'Vũ Văn F',
        email: 'vuvanf@company.com'
      },
      createdAt: '2025-10-04T08:30:00Z',
      updatedAt: '2025-10-06T10:15:00Z',
      dueDate: '2025-10-06T20:00:00Z',
      slaBreached: true,
      tags: ['bug', 'critical', 'data'],
      attachments: [],
      comments: []
    },
    {
      id: 'TCK-004',
      title: 'Sự cố máy chủ database',
      description: 'Database không phản hồi, ảnh hưởng đến toàn bộ hệ thống',
      customer: {
        id: 4,
        name: 'Phạm Văn G',
        email: 'phamvang@email.com',
        phone: '0908765432'
      },
      priority: 'high',
      status: 'new',
      category: 'technical',
      assignedTo: {
        id: 1,
        name: 'Trần Thị B',
        email: 'tranthib@company.com'
      },
      createdAt: '2025-10-06T15:00:00Z',
      updatedAt: '2025-10-06T15:00:00Z',
      dueDate: '2025-10-07T15:00:00Z',
      slaBreached: false,
      tags: ['database', 'server', 'critical-system'],
      attachments: [],
      comments: []
    },
    {
      id: 'TCK-005',
      title: 'Cập nhật thông tin tài khoản',
      description: 'Khách hàng yêu cầu thay đổi thông tin liên hệ và quyền truy cập',
      customer: {
        id: 5,
        name: 'Trần Thị H',
        email: 'tranthih@email.com',
        phone: '0907654321'
      },
      priority: 'medium',
      status: 'open',
      category: 'account',
      assignedTo: {
        id: 2,
        name: 'Phạm Văn D',
        email: 'phamvand@company.com'
      },
      createdAt: '2025-10-05T11:00:00Z',
      updatedAt: '2025-10-05T11:00:00Z',
      dueDate: '2025-10-07T17:00:00Z',
      slaBreached: false,
      tags: ['account', 'profile', 'access'],
      attachments: [],
      comments: []
    },
    {
      id: 'TCK-006',
      title: 'Vấn đề thanh toán',
      description: 'Giao dịch thanh toán không thành công, cần kiểm tra và xử lý',
      customer: {
        id: 6,
        name: 'Nguyễn Văn I',
        email: 'nguyenvani@email.com',
        phone: '0906543210'
      },
      priority: 'medium',
      status: 'pending',
      category: 'billing',
      assignedTo: {
        id: 3,
        name: 'Vũ Văn F',
        email: 'vuvanf@company.com'
      },
      createdAt: '2025-10-05T16:30:00Z',
      updatedAt: '2025-10-06T09:00:00Z',
      dueDate: '2025-10-07T16:30:00Z',
      slaBreached: false,
      tags: ['payment', 'transaction', 'billing'],
      attachments: [],
      comments: []
    },
    {
      id: 'TCK-007',
      title: 'Câu hỏi về tính năng',
      description: 'Khách hàng cần tư vấn về cách sử dụng các tính năng mới',
      customer: {
        id: 7,
        name: 'Lê Thị J',
        email: 'lethij@email.com',
        phone: '0905432109'
      },
      priority: 'low',
      status: 'new',
      category: 'general',
      assignedTo: {
        id: 1,
        name: 'Trần Thị B',
        email: 'tranthib@company.com'
      },
      createdAt: '2025-10-06T13:00:00Z',
      updatedAt: '2025-10-06T13:00:00Z',
      dueDate: '2025-10-09T13:00:00Z',
      slaBreached: false,
      tags: ['question', 'features', 'consultation'],
      attachments: [],
      comments: []
    },
    {
      id: 'TCK-008',
      title: 'Yêu cầu hướng dẫn sử dụng',
      description: 'Khách hàng mới cần hướng dẫn chi tiết về cách sử dụng hệ thống',
      customer: {
        id: 8,
        name: 'Hoàng Văn K',
        email: 'hoangvank@email.com',
        phone: '0904321098'
      },
      priority: 'low',
      status: 'open',
      category: 'general',
      assignedTo: {
        id: 2,
        name: 'Phạm Văn D',
        email: 'phamvand@company.com'
      },
      createdAt: '2025-10-05T08:00:00Z',
      updatedAt: '2025-10-05T08:00:00Z',
      dueDate: '2025-10-08T08:00:00Z',
      slaBreached: false,
      tags: ['tutorial', 'guidance', 'new-user'],
      attachments: [],
      comments: []
    },
    {
      id: 'TCK-009',
      title: 'Góp ý cải tiến giao diện',
      description: 'Khách hàng đề xuất cải tiến giao diện để dễ sử dụng hơn',
      customer: {
        id: 9,
        name: 'Vũ Thị L',
        email: 'vuthil@email.com',
        phone: '0903210987'
      },
      priority: 'low',
      status: 'open',
      category: 'feature_request',
      assignedTo: {
        id: 3,
        name: 'Vũ Văn F',
        email: 'vuvanf@company.com'
      },
      createdAt: '2025-10-04T10:00:00Z',
      updatedAt: '2025-10-04T10:00:00Z',
      dueDate: '2025-10-07T10:00:00Z',
      slaBreached: false,
      tags: ['ui', 'improvement', 'feedback'],
      attachments: [],
      comments: []
    }
    // Thêm các tickets khác...
  ]);

  const [currentTicket, setCurrentTicket] = useState(null);
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    customer: { id: '', name: '', email: '', phone: '' },
    priority: 'medium',
    category: 'technical',
    assignedTo: { id: '', name: '', email: '' },
    dueDate: '',
    tags: [],
    status: 'new'
  });

  const customers = [
    { id: 1, name: 'Nguyễn Văn A', email: 'nguyenvana@email.com' },
    { id: 2, name: 'Lê Văn C', email: 'levanc@email.com' },
    { id: 3, name: 'Trần Thị H', email: 'tranthih@email.com' }
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

  useEffect(() => {
    // Tìm ticket theo ID từ URL
    const ticket = tickets.find(t => t.id === ticketId);
    if (ticket) {
      setCurrentTicket(ticket);
      setFormData({
        title: ticket.title,
        description: ticket.description,
        customer: ticket.customer,
        priority: ticket.priority,
        category: ticket.category,
        assignedTo: ticket.assignedTo,
        dueDate: ticket.dueDate ? ticket.dueDate.slice(0, 16) : '',
        tags: ticket.tags,
        status: ticket.status
      });
    }
  }, [ticketId, tickets]);

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
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
    // Trong thực tế sẽ gọi API để cập nhật ticket
    console.log('Updated ticket:', { ...currentTicket, ...formData });
    navigate('/helpdesk');
  };

  const handleCancel = () => {
    navigate('/helpdesk');
  };

  if (!currentTicket) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <h2 className="text-xl font-semibold text-gray-900 mb-2">Ticket không tồn tại</h2>
          <p className="text-gray-600 mb-4">Không tìm thấy ticket với ID: {ticketId}</p>
          <button
            onClick={() => navigate('/helpdesk')}
            className="bg-indigo-600 text-white px-4 py-2 rounded-md hover:bg-indigo-700"
          >
            Quay lại Helpdesk
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 py-6">
      <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
        {/* Header */}
        <div className="mb-6">
          <div className="flex items-center mb-4">
            <button
              onClick={handleCancel}
              className="mr-4 p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-lg transition-colors"
            >
              <ArrowLeftIcon className="h-5 w-5" />
            </button>
            <div>
              <h1 className="text-2xl font-bold text-gray-900">
                Chỉnh sửa Ticket - {currentTicket.id}
              </h1>
              <p className="text-gray-600 mt-1">
                Cập nhật thông tin ticket
              </p>
            </div>
          </div>
        </div>

        {/* Form */}
        <div className="bg-white shadow-lg rounded-lg">
          <div className="px-6 py-4 border-b border-gray-200">
            <h2 className="text-lg font-medium text-gray-900">Thông tin ticket</h2>
          </div>
          
          <form onSubmit={handleSubmit} className="p-6 space-y-6">
            {/* Ticket Title */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Tiêu đề ticket *
              </label>
              <input
                type="text"
                name="title"
                value={formData.title}
                onChange={handleInputChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
                placeholder="Nhập tiêu đề ticket"
                required
              />
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Customer Selection */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Khách hàng *
                </label>
                <div className="relative">
                  <select
                    value={formData.customer.id}
                    onChange={handleCustomerChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 appearance-none bg-white"
                    required
                  >
                    <option value="">Chọn khách hàng</option>
                    {customers.map(customer => (
                      <option key={customer.id} value={customer.id}>
                        {customer.name} - {customer.email}
                      </option>
                    ))}
                  </select>
                  <ChevronDownIcon className="absolute right-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400 pointer-events-none" />
                </div>
              </div>

              {/* Priority Selection */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Độ ưu tiên *
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
              </div>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Category Selection */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Danh mục *
                </label>
                <div className="relative">
                  <select
                    name="category"
                    value={formData.category}
                    onChange={handleInputChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 appearance-none bg-white"
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
                <label className="block text-sm font-medium text-gray-700 mb-2">
                  Phân công cho
                </label>
                <div className="relative">
                  <select
                    value={formData.assignedTo.id}
                    onChange={handleAssignedToChange}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 appearance-none bg-white"
                  >
                    <option value="">Chọn người xử lý</option>
                    {helpdeskAgents.map(agent => (
                      <option key={agent.id} value={agent.id}>
                        {agent.name} - {agent.email}
                      </option>
                    ))}
                  </select>
                  <ChevronDownIcon className="absolute right-3 top-1/2 transform -translate-y-1/2 h-5 w-5 text-gray-400 pointer-events-none" />
                </div>
              </div>
            </div>

            {/* Description */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Mô tả chi tiết *
              </label>
              <textarea
                name="description"
                value={formData.description}
                onChange={handleInputChange}
                placeholder="Mô tả chi tiết vấn đề hoặc yêu cầu..."
                rows={6}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 resize-none"
                required
              />
            </div>

            {/* Due Date */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Hạn xử lý
              </label>
              <input
                type="datetime-local"
                name="dueDate"
                value={formData.dueDate}
                onChange={handleInputChange}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
              />
            </div>

            {/* Form Actions */}
            <div className="flex justify-end space-x-3 pt-6 border-t border-gray-200">
              <button
                type="button"
                onClick={handleCancel}
                className="px-4 py-2 bg-gray-300 text-gray-700 rounded-md hover:bg-gray-400 transition duration-200"
              >
                Hủy
              </button>
              <button
                type="submit"
                className="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 transition duration-200"
              >
                Cập nhật Ticket
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default TicketEditPage;