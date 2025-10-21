import React from 'react'
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

const PriorityTicketCard = ({ priority, tickets, onViewAll, onStatusChange, onViewTicket }) => {
  const priorityConfig = {
    low: {
      title: 'Low',
      color: 'bg-green-500',
      textColor: 'text-green-600',
      bgColor: 'bg-green-50',
      borderColor: 'border-green-200',
      hoverColor: 'hover:bg-green-100'
    },
    medium: {
      title: 'Medium',
      color: 'bg-yellow-500',
      textColor: 'text-yellow-600',
      bgColor: 'bg-yellow-50',
      borderColor: 'border-yellow-200',
      hoverColor: 'hover:bg-yellow-100'
    },
    high: {
      title: 'High',
      color: 'bg-red-500',
      textColor: 'text-red-600',
      bgColor: 'bg-red-50',
      borderColor: 'border-red-200',
      hoverColor: 'hover:bg-red-100'
    }
  };

  const config = priorityConfig[priority];
  const priorityTickets = tickets.filter(ticket => ticket.priority === priority);
  const displayTickets = priorityTickets.slice(0, 5);

  const getStatusInfo = (status) => {
    const statuses = {
      new: { name: 'Mới', color: 'bg-blue-100 text-blue-800' },
      open: { name: 'Đang mở', color: 'bg-yellow-100 text-yellow-800' },
      in_progress: { name: 'Đang xử lý', color: 'bg-orange-100 text-orange-800' },
      pending: { name: 'Chờ phản hồi', color: 'bg-purple-100 text-purple-800' },
      escalated: { name: 'Leo thang', color: 'bg-red-100 text-red-800' },
      resolved: { name: 'Đã giải quyết', color: 'bg-green-100 text-green-800' },
      closed: { name: 'Đã đóng', color: 'bg-gray-100 text-gray-800' }
    };
    return statuses[status] || statuses.new;
  };

  const getCategoryInfo = (category) => {
    const categories = {
      technical: { name: 'Kỹ thuật', icon: '🔧' },
      bug: { name: 'Lỗi hệ thống', icon: '🐛' },
      feature_request: { name: 'Yêu cầu tính năng', icon: '💡' },
      account: { name: 'Tài khoản', icon: '👤' },
      billing: { name: 'Thanh toán', icon: '💳' },
      general: { name: 'Tổng quát', icon: '📋' }
    };
    return categories[category] || categories.general;
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString('vi-VN', {
      month: 'short',
      day: 'numeric'
    });
  };

  return (
    <div className={`bg-white rounded-lg shadow-lg border-t-4 ${config.borderColor} h-[600px] flex flex-col  `}>
      {/* Card Header */}
      <div className={`${config.bgColor} px-6 py-4 border-b border-gray-200 flex-shrink-0`}>
        <div className="flex items-center justify-between">
          <div className="flex items-center">
            <div className={`w-4 h-4 ${config.color} rounded-full mr-3`}></div>
            <h3 className={`text-lg font-semibold ${config.textColor}`}>
              {config.title}
            </h3>
          </div>
          <div className="flex items-center space-x-2">
            <span className={`px-2 py-1 text-xs font-medium ${config.color} text-white rounded-full`}>
              {priorityTickets.length}
            </span>
            <button
              onClick={() => onViewAll(priority)}
              className={`text-xs ${config.textColor} hover:opacity-75 cursor-pointer transition-opacity flex items-center`}
            >
              <ChevronDownIcon className="h-4 w-4 rotate-[-90deg]" />
            </button>
          </div>
        </div>
      </div>

      {/* Tickets List */}
      <div className="flex-1 overflow-y-auto p-4">
        {displayTickets.length > 0 ? (
          <div className="space-y-3">
            {displayTickets.map((ticket) => {
              const statusInfo = getStatusInfo(ticket.status);
              const categoryInfo = getCategoryInfo(ticket.category);

              return (
                <div 
                  key={ticket.id} 
                  className={`p-4 border border-gray-200 rounded-lg ${config.hoverColor} transition-colors cursor-pointer`}
                  onClick={() => onViewTicket(ticket)}
                >
                  {/* Ticket Header */}
                  <div className="flex items-start justify-between mb-2">
                    <div className="flex items-center space-x-2">
                      <span className="text-xs font-mono text-gray-500">{ticket.id}</span>
                      <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${statusInfo.color}`}>
                        {statusInfo.name}
                      </span>
                    </div>
                    <div className="flex items-center space-x-1">
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          onViewTicket(ticket);
                        }}
                        className="text-gray-400 hover:text-indigo-600 transition-colors cursor-pointer"
                        title="View Details"
                      >
                        <EyeIcon className="h-4 w-4" />
                      </button>
                    </div>
                  </div>

                  {/* Ticket Content */}
                  <div className="mb-3">
                    <h4 className="text-sm font-medium text-gray-900 line-clamp-2 mb-1">
                      {ticket.title}
                    </h4>
                    <p className="text-xs text-gray-600 line-clamp-2">
                      {ticket.description}
                    </p>
                  </div>

                  {/* Ticket Meta */}
                  <div className="flex items-center justify-between text-xs text-gray-500">
                    <div className="flex items-center space-x-2">
                      <span className="flex items-center">
                        <span className="mr-1">{categoryInfo.icon}</span>
                        {categoryInfo.name}
                      </span>
                      <span className="flex items-center">
                        <span className="flex items-center space-x-0.5">
                          {Array.from({ length: 5 }, (_, i) => (
                            <span key={i} className={`${i < (ticket.stars || 1) ? 'text-yellow-500' : 'text-gray-300'}`}>
                              {i < (ticket.stars || 1) ? (
                                <StarIconSolid className="h-3 w-3" />
                              ) : (
                                <StarIcon className="h-3 w-3" />
                              )}
                            </span>
                          ))}
                        </span>
                        <span className="ml-1 text-xs text-gray-600">{ticket.stars || 1}</span>
                      </span>
                    </div>
                    <div className="flex flex-col items-end">
                      <span className="font-medium">{ticket.customer.name}</span>
                      <span>{formatDate(ticket.createdAt)}</span>
                    </div>
                  </div>

                  {/* Assigned To */}
                  <div className="mt-2 pt-2 border-t border-gray-100">
                    <div className="flex items-center justify-between text-xs">
                      <span className="text-gray-500">Assigned:</span>
                      <span className="font-medium text-gray-700">
                        {ticket.assignedTo?.name || 'Unassigned'}
                      </span>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        ) : (
          <div className="flex flex-col items-center justify-center h-full text-center">
            <div className={`mx-auto h-12 w-12 ${config.color} rounded-full flex items-center justify-center mb-4 opacity-20`}>
              <TagIcon className="h-6 w-6 text-white" />
            </div>
            <h3 className="text-sm font-medium text-gray-900 mb-1">No {priority} priority tickets</h3>
            <p className="text-xs text-gray-500">All tickets have been resolved</p>
          </div>
        )}
      </div>

      {/* Card Footer */}
      <div className={`${config.bgColor} px-4 py-3 border-t border-gray-200 flex-shrink-0`}>
        <button
          onClick={() => onViewAll(priority)}
          className={`w-full text-center text-sm font-medium ${config.textColor} hover:opacity-75 cursor-pointer transition-opacity`}
        >
          View All ({priorityTickets.length}) →
        </button>
      </div>
    </div>
  );
};

export default PriorityTicketCard
