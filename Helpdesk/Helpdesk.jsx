import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  PlusIcon,
  ExclamationTriangleIcon,
  CheckCircleIcon,
  XCircleIcon,
  ClockIcon,
  TagIcon,
  ChevronDownIcon,
  EyeIcon,
  ArrowTrendingUpIcon,
  ArrowTrendingDownIcon,
  MinusIcon,
  StarIcon
} from '@heroicons/react/24/outline';
import { StarIcon as StarIconSolid } from '@heroicons/react/24/solid';
import TicketForm from './TicketForm';
import ViewAllTicketsModal from './ViewAllTicketsModal';
import TicketDetailModal from './TicketDetailModal';
import PriorityTicketCard from './PriorityTicketCard';


const Helpdesk = () => {

    const navigate = useNavigate();

  // ==================== STATES ====================
  const [tickets, setTickets] = useState([
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
      stars: 4,
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
      stars: 2,
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
    },
    {
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
      stars: 5,
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
      stars: 4,
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
      stars: 2,
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
      stars: 3,
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
      stars: 1,
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
      stars: 1,
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
      stars: 1,
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
  ]);

  // Modal states
  const [isTicketModalOpen, setIsTicketModalOpen] = useState(false);
  const [selectedTicket, setSelectedTicket] = useState(null);
  const [isTicketDetailModalOpen, setIsTicketDetailModalOpen] = useState(false);
  const [isViewAllModalOpen, setIsViewAllModalOpen] = useState(false);
  const [defaultPriorityFilter, setDefaultPriorityFilter] = useState('all');

  // ==================== CONSTANTS ====================
  const STATUSES = [
    { id: 'new', name: 'Mới', color: 'bg-blue-100 text-blue-800' },
    { id: 'open', name: 'Đang mở', color: 'bg-yellow-100 text-yellow-800' },
    { id: 'in_progress', name: 'Đang xử lý', color: 'bg-orange-100 text-orange-800' },
    { id: 'pending', name: 'Chờ phản hồi', color: 'bg-purple-100 text-purple-800' },
    { id: 'escalated', name: 'Leo thang', color: 'bg-red-100 text-red-800' },
    { id: 'resolved', name: 'Đã giải quyết', color: 'bg-green-100 text-green-800' },
    { id: 'closed', name: 'Đã đóng', color: 'bg-gray-100 text-gray-800' }
  ];

  const PRIORITIES = [
    { id: 'low', name: 'Thấp', color: 'bg-green-100 text-green-800', sla: 72 },
    { id: 'medium', name: 'Trung bình', color: 'bg-yellow-100 text-yellow-800', sla: 48 },
    { id: 'high', name: 'Cao', color: 'bg-orange-100 text-orange-800', sla: 24 },
    { id: 'critical', name: 'Khẩn cấp', color: 'bg-red-100 text-red-800', sla: 4 }
  ];

  const CATEGORIES = [
    { id: 'technical', name: 'Kỹ thuật', icon: '🔧' },
    { id: 'bug', name: 'Lỗi hệ thống', icon: '🐛' },
    { id: 'feature_request', name: 'Yêu cầu tính năng', icon: '💡' },
    { id: 'account', name: 'Tài khoản', icon: '👤' },
    { id: 'billing', name: 'Thanh toán', icon: '💳' },
    { id: 'general', name: 'Tổng quát', icon: '📋' }
  ];

  const AGENTS = [
    { id: 1, name: 'Trần Thị B', email: 'tranthib@company.com', department: 'Technical' },
    { id: 2, name: 'Phạm Văn D', email: 'phamvand@company.com', department: 'Product' },
    { id: 3, name: 'Vũ Văn F', email: 'vuvanf@company.com', department: 'Technical' }
  ];

  // ==================== EVENT HANDLERS ====================
  const handleViewTicket = (ticket) => {
    setSelectedTicket(ticket);
    setIsTicketDetailModalOpen(true);
  };

  const handleEditTicket = (ticket) => {
    setSelectedTicket(ticket);
    setIsTicketModalOpen(true);
  };

  // const handleCreateTicket = () => {
  //   setSelectedTicket(null);
  //   setIsTicketModalOpen(true);
  // };

  const handleStatusChange = (ticketId, newStatus) => {
    setTickets(tickets.map(ticket =>
      ticket.id === ticketId
        ? { 
            ...ticket, 
            status: newStatus, 
            updatedAt: new Date().toISOString(),
            ...(newStatus === 'resolved' && { resolvedAt: new Date().toISOString() }),
            ...(newStatus === 'closed' && { closedAt: new Date().toISOString() })
          }
        : ticket
    ));
  };

  const handleAssignTicket = (ticketId, agentId) => {
    const agent = AGENTS.find(a => a.id === parseInt(agentId));
    setTickets(tickets.map(ticket =>
      ticket.id === ticketId
        ? { 
            ...ticket, 
            assignedTo: agent,
            updatedAt: new Date().toISOString()
          }
        : ticket
    ));
  };

  const handlePriorityChange = (ticketId, newPriority) => {
    setTickets(tickets.map(ticket =>
      ticket.id === ticketId
        ? { 
            ...ticket, 
            priority: newPriority,
            updatedAt: new Date().toISOString()
          }
        : ticket
    ));
  };

  const handleViewAllTickets = (priority = 'all') => {
    setDefaultPriorityFilter(priority);
    setIsViewAllModalOpen(true);
  };

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      {/* Header */}
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Helpdesk</h1>
          <p className="mt-2 text-gray-600">Ticket Priority Management</p>
        </div>
        <button 
          onClick={() => { navigate('/helpdesk/create') }}
          className="bg-indigo-600 text-white px-4 py-2 rounded-lg hover:bg-indigo-700 transition-colors flex items-center space-x-2"
        >
          <PlusIcon className="h-5 w-5" />
          <span>Create Ticket</span>
        </button>
      </div>

      {/* Priority Overview Statistics */}
      <div className="mb-8">
        <HelpdeskOverview tickets={tickets} />
      </div>

      {/* Priority Ticket Cards */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 p-8 bg-gray-200 rounded-lg">
        <PriorityTicketCard 
          priority="low" 
          tickets={tickets} 
          onViewAll={handleViewAllTickets}
          onStatusChange={handleStatusChange}
          onViewTicket={handleViewTicket}
        />
        <PriorityTicketCard 
          priority="medium" 
          tickets={tickets} 
          onViewAll={handleViewAllTickets}
          onStatusChange={handleStatusChange}
          onViewTicket={handleViewTicket}
        />
        <PriorityTicketCard 
          priority="high" 
          tickets={tickets} 
          onViewAll={handleViewAllTickets}
          onStatusChange={handleStatusChange}
          onViewTicket={handleViewTicket}
        />
      </div>

      {/* Modals */}
      {isTicketModalOpen && (
        <TicketForm
          isOpen={isTicketModalOpen}
          onClose={() => setIsTicketModalOpen(false)}
          ticket={selectedTicket}
          onSubmit={(ticketData) => {
            if (selectedTicket) {
              setTickets(tickets.map(t => 
                t.id === selectedTicket.id 
                  ? { ...t, ...ticketData, updatedAt: new Date().toISOString() }
                  : t
              ));
            } else {
              const newTicket = {
                id: `TCK-${String(tickets.length + 1).padStart(3, '0')}`,
                ...ticketData,
                createdAt: new Date().toISOString(),
                updatedAt: new Date().toISOString(),
                comments: []
              };
              setTickets([...tickets, newTicket]);
            }
            setIsTicketModalOpen(false);
          }}
        />
      )}

      {isViewAllModalOpen && (
        <ViewAllTicketsModal
          isOpen={isViewAllModalOpen}
          onClose={() => setIsViewAllModalOpen(false)}
          tickets={tickets}
          defaultPriorityFilter={defaultPriorityFilter}
          statuses={STATUSES}
          priorities={PRIORITIES}
          categories={CATEGORIES}
          agents={AGENTS}
          onStatusChange={handleStatusChange}
          onAssignTicket={handleAssignTicket}
          onPriorityChange={handlePriorityChange}
          onViewTicket={handleViewTicket}
          onEditTicket={handleEditTicket}
        />
      )}

      {isTicketDetailModalOpen && (
        <TicketDetailModal
          isOpen={isTicketDetailModalOpen}
          onClose={() => setIsTicketDetailModalOpen(false)}
          ticket={selectedTicket}
          onStatusChange={handleStatusChange}
          onEdit={handleEditTicket}
        />
      )}
    </div>
  );
};

// Helpdesk Overview Component
const HelpdeskOverview = ({ tickets }) => {
  const stats = {
    total: tickets.length,
    high: tickets.filter(t => t.priority === 'high').length,
    medium: tickets.filter(t => t.priority === 'medium').length,
    low: tickets.filter(t => t.priority === 'low').length,
    critical: tickets.filter(t => t.priority === 'critical').length,
    open: tickets.filter(t => ['new', 'open', 'in_progress'].includes(t.status)).length,
    resolved: tickets.filter(t => t.status === 'resolved').length,
    slaBreached: tickets.filter(t => t.slaBreached).length,
  };

  const priorityCards = [
    {
      title: 'Critical Priority',
      count: stats.critical,
      color: 'bg-red-500',
      textColor: 'text-red-600',
      bgColor: 'bg-red-50',
      icon: ExclamationTriangleIcon,
      trend: '+12%'
    },
    {
      title: 'High Priority',
      count: stats.high,
      color: 'bg-orange-500',
      textColor: 'text-orange-600',
      bgColor: 'bg-orange-50',
      icon: ArrowTrendingUpIcon,
      trend: '+5%'
    },
    {
      title: 'Medium Priority',
      count: stats.medium,
      color: 'bg-yellow-500',
      textColor: 'text-yellow-600',
      bgColor: 'bg-yellow-50',
      icon: MinusIcon,
      trend: '-2%'
    },
    {
      title: 'Low Priority',
      count: stats.low,
      color: 'bg-green-500',
      textColor: 'text-green-600',
      bgColor: 'bg-green-50',
      icon: ArrowTrendingDownIcon,
      trend: '-8%'
    }
  ];

  const overallStats = [
    {
      title: 'Total Tickets',
      value: stats.total,
      icon: TagIcon,
      color: 'text-blue-600',
      bgColor: 'bg-blue-50'
    },
    {
      title: 'Open Tickets',
      value: stats.open,
      icon: ClockIcon,
      color: 'text-purple-600',
      bgColor: 'bg-purple-50'
    },
    {
      title: 'Resolved',
      value: stats.resolved,
      icon: CheckCircleIcon,
      color: 'text-green-600',
      bgColor: 'bg-green-50'
    },
    {
      title: 'SLA Breached',
      value: stats.slaBreached,
      icon: XCircleIcon,
      color: 'text-red-600',
      bgColor: 'bg-red-50'
    }
  ];

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
      {/* Priority Statistics */}
      <div>
        <h2 className="text-xl font-semibold text-gray-900 mb-4">Priority Overview</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {priorityCards.map((card, index) => (
            <div key={index} className={`${card.bgColor} rounded-lg p-6 border border-gray-100`}>
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-gray-600">{card.title}</p>
                  <p className={`text-3xl font-bold ${card.textColor} mt-2`}>{card.count}</p>
                  <p className="text-sm text-gray-500 mt-1">vs last week {card.trend}</p>
                </div>
                {/* <div className={`p-3 rounded-full ${card.color}`}>
                  <card.icon className="h-6 w-6 text-white" />
                </div> */}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Overall Statistics */}
      <div>
        <h2 className="text-xl font-semibold text-gray-900 mb-4">Overall Statistics</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {overallStats.map((stat, index) => (
            <div key={index} className={`${stat.bgColor} rounded-lg p-6 border border-gray-100`}>
              <div className="flex items-center">
                <div className={`p-2 rounded-md ${stat.color} bg-white`}>
                  <stat.icon className="h-5 w-5" />
                </div>
                <div className="ml-4">
                  <p className="text-sm font-medium text-gray-600">{stat.title}</p>
                  <p className={`text-2xl font-bold ${stat.color}`}>{stat.value}</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

// // Priority Ticket Card Component
// const PriorityTicketCard = ({ priority, tickets, onViewAll, onStatusChange, onViewTicket }) => {
//   const priorityConfig = {
//     low: {
//       title: 'Low',
//       color: 'bg-green-500',
//       textColor: 'text-green-600',
//       bgColor: 'bg-green-50',
//       borderColor: 'border-green-200',
//       hoverColor: 'hover:bg-green-100'
//     },
//     medium: {
//       title: 'Medium',
//       color: 'bg-yellow-500',
//       textColor: 'text-yellow-600',
//       bgColor: 'bg-yellow-50',
//       borderColor: 'border-yellow-200',
//       hoverColor: 'hover:bg-yellow-100'
//     },
//     high: {
//       title: 'High',
//       color: 'bg-red-500',
//       textColor: 'text-red-600',
//       bgColor: 'bg-red-50',
//       borderColor: 'border-red-200',
//       hoverColor: 'hover:bg-red-100'
//     }
//   };

//   const config = priorityConfig[priority];
//   const priorityTickets = tickets.filter(ticket => ticket.priority === priority);
//   const displayTickets = priorityTickets.slice(0, 5);

//   const getStatusInfo = (status) => {
//     const statuses = {
//       new: { name: 'Mới', color: 'bg-blue-100 text-blue-800' },
//       open: { name: 'Đang mở', color: 'bg-yellow-100 text-yellow-800' },
//       in_progress: { name: 'Đang xử lý', color: 'bg-orange-100 text-orange-800' },
//       pending: { name: 'Chờ phản hồi', color: 'bg-purple-100 text-purple-800' },
//       escalated: { name: 'Leo thang', color: 'bg-red-100 text-red-800' },
//       resolved: { name: 'Đã giải quyết', color: 'bg-green-100 text-green-800' },
//       closed: { name: 'Đã đóng', color: 'bg-gray-100 text-gray-800' }
//     };
//     return statuses[status] || statuses.new;
//   };

//   const getCategoryInfo = (category) => {
//     const categories = {
//       technical: { name: 'Kỹ thuật', icon: '🔧' },
//       bug: { name: 'Lỗi hệ thống', icon: '🐛' },
//       feature_request: { name: 'Yêu cầu tính năng', icon: '💡' },
//       account: { name: 'Tài khoản', icon: '👤' },
//       billing: { name: 'Thanh toán', icon: '💳' },
//       general: { name: 'Tổng quát', icon: '📋' }
//     };
//     return categories[category] || categories.general;
//   };

//   const formatDate = (dateString) => {
//     return new Date(dateString).toLocaleDateString('vi-VN', {
//       month: 'short',
//       day: 'numeric'
//     });
//   };

//   return (
//     <div className={`bg-white rounded-lg shadow-lg border-t-4 ${config.borderColor} h-[600px] flex flex-col  `}>
//       {/* Card Header */}
//       <div className={`${config.bgColor} px-6 py-4 border-b border-gray-200 flex-shrink-0`}>
//         <div className="flex items-center justify-between">
//           <div className="flex items-center">
//             <div className={`w-4 h-4 ${config.color} rounded-full mr-3`}></div>
//             <h3 className={`text-lg font-semibold ${config.textColor}`}>
//               {config.title}
//             </h3>
//           </div>
//           <div className="flex items-center space-x-2">
//             <span className={`px-2 py-1 text-xs font-medium ${config.color} text-white rounded-full`}>
//               {priorityTickets.length}
//             </span>
//             <button
//               onClick={() => onViewAll(priority)}
//               className={`text-xs ${config.textColor} hover:opacity-75 cursor-pointer transition-opacity flex items-center`}
//             >
//               <ChevronDownIcon className="h-4 w-4 rotate-[-90deg]" />
//             </button>
//           </div>
//         </div>
//       </div>

//       {/* Tickets List */}
//       <div className="flex-1 overflow-y-auto p-4">
//         {displayTickets.length > 0 ? (
//           <div className="space-y-3">
//             {displayTickets.map((ticket) => {
//               const statusInfo = getStatusInfo(ticket.status);
//               const categoryInfo = getCategoryInfo(ticket.category);

//               return (
//                 <div 
//                   key={ticket.id} 
//                   className={`p-4 border border-gray-200 rounded-lg ${config.hoverColor} transition-colors cursor-pointer`}
//                   onClick={() => onViewTicket(ticket)}
//                 >
//                   {/* Ticket Header */}
//                   <div className="flex items-start justify-between mb-2">
//                     <div className="flex items-center space-x-2">
//                       <span className="text-xs font-mono text-gray-500">{ticket.id}</span>
//                       <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${statusInfo.color}`}>
//                         {statusInfo.name}
//                       </span>
//                     </div>
//                     <div className="flex items-center space-x-1">
//                       <button
//                         onClick={(e) => {
//                           e.stopPropagation();
//                           onViewTicket(ticket);
//                         }}
//                         className="text-gray-400 hover:text-indigo-600 transition-colors cursor-pointer"
//                         title="View Details"
//                       >
//                         <EyeIcon className="h-4 w-4" />
//                       </button>
//                     </div>
//                   </div>

//                   {/* Ticket Content */}
//                   <div className="mb-3">
//                     <h4 className="text-sm font-medium text-gray-900 line-clamp-2 mb-1">
//                       {ticket.title}
//                     </h4>
//                     <p className="text-xs text-gray-600 line-clamp-2">
//                       {ticket.description}
//                     </p>
//                   </div>

//                   {/* Ticket Meta */}
//                   <div className="flex items-center justify-between text-xs text-gray-500">
//                     <div className="flex items-center space-x-2">
//                       <span className="flex items-center">
//                         <span className="mr-1">{categoryInfo.icon}</span>
//                         {categoryInfo.name}
//                       </span>
//                       <span className="flex items-center">
//                         <span className="flex items-center space-x-0.5">
//                           {Array.from({ length: 5 }, (_, i) => (
//                             <span key={i} className={`${i < (ticket.stars || 1) ? 'text-yellow-500' : 'text-gray-300'}`}>
//                               {i < (ticket.stars || 1) ? (
//                                 <StarIconSolid className="h-3 w-3" />
//                               ) : (
//                                 <StarIcon className="h-3 w-3" />
//                               )}
//                             </span>
//                           ))}
//                         </span>
//                         <span className="ml-1 text-xs text-gray-600">{ticket.stars || 1}</span>
//                       </span>
//                     </div>
//                     <div className="flex flex-col items-end">
//                       <span className="font-medium">{ticket.customer.name}</span>
//                       <span>{formatDate(ticket.createdAt)}</span>
//                     </div>
//                   </div>

//                   {/* Assigned To */}
//                   <div className="mt-2 pt-2 border-t border-gray-100">
//                     <div className="flex items-center justify-between text-xs">
//                       <span className="text-gray-500">Assigned:</span>
//                       <span className="font-medium text-gray-700">
//                         {ticket.assignedTo?.name || 'Unassigned'}
//                       </span>
//                     </div>
//                   </div>
//                 </div>
//               );
//             })}
//           </div>
//         ) : (
//           <div className="flex flex-col items-center justify-center h-full text-center">
//             <div className={`mx-auto h-12 w-12 ${config.color} rounded-full flex items-center justify-center mb-4 opacity-20`}>
//               <TagIcon className="h-6 w-6 text-white" />
//             </div>
//             <h3 className="text-sm font-medium text-gray-900 mb-1">No {priority} priority tickets</h3>
//             <p className="text-xs text-gray-500">All tickets have been resolved</p>
//           </div>
//         )}
//       </div>

//       {/* Card Footer */}
//       <div className={`${config.bgColor} px-4 py-3 border-t border-gray-200 flex-shrink-0`}>
//         <button
//           onClick={() => onViewAll(priority)}
//           className={`w-full text-center text-sm font-medium ${config.textColor} hover:opacity-75 cursor-pointer transition-opacity`}
//         >
//           View All ({priorityTickets.length}) →
//         </button>
//       </div>
//     </div>
//   );
// };

export default Helpdesk;
