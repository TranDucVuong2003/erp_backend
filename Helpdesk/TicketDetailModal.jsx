import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  XMarkIcon,
  UserIcon,
  CalendarIcon,
  TagIcon,
  ClockIcon,
  ExclamationTriangleIcon,
  CheckCircleIcon,
  XCircleIcon,
  ChatBubbleLeftRightIcon,
  PaperClipIcon,
  PencilIcon,
  ArrowPathIcon,
  StarIcon
} from '@heroicons/react/24/outline';
import { StarIcon as StarIconSolid } from '@heroicons/react/24/solid';

const TicketDetailModal = ({ isOpen, onClose, ticket, onStatusChange, onEdit }) => {
  const navigate = useNavigate();
  const [newComment, setNewComment] = useState('');
  const [comments, setComments] = useState(ticket?.comments || []);

  if (!isOpen || !ticket) return null;

  const getPriorityConfig = (priority) => {
    const configs = {
      low: { 
        icon: <ArrowPathIcon className="h-4 w-4" />, 
        color: 'text-green-600', 
        bgColor: 'bg-green-100',
        label: 'Thấp' 
      },
      medium: { 
        icon: <ClockIcon className="h-4 w-4" />, 
        color: 'text-yellow-600', 
        bgColor: 'bg-yellow-100',
        label: 'Trung bình' 
      },
      high: { 
        icon: <ExclamationTriangleIcon className="h-4 w-4" />, 
        color: 'text-orange-600', 
        bgColor: 'bg-orange-100',
        label: 'Cao' 
      },
      critical: { 
        icon: <ExclamationTriangleIcon className="h-4 w-4" />, 
        color: 'text-red-600', 
        bgColor: 'bg-red-100',
        label: 'Khẩn cấp' 
      }
    };
    return configs[priority] || configs.medium;
  };

  const getStatusConfig = (status) => {
    const configs = {
      new: { 
        icon: <TagIcon className="h-4 w-4" />, 
        color: 'text-blue-600', 
        bgColor: 'bg-blue-100',
        label: 'Mới' 
      },
      open: { 
        icon: <ClockIcon className="h-4 w-4" />, 
        color: 'text-orange-600', 
        bgColor: 'bg-orange-100',
        label: 'Đang xử lý' 
      },
      in_progress: { 
        icon: <ArrowPathIcon className="h-4 w-4" />, 
        color: 'text-yellow-600', 
        bgColor: 'bg-yellow-100',
        label: 'Đang thực hiện' 
      },
      pending: { 
        icon: <ClockIcon className="h-4 w-4" />, 
        color: 'text-gray-600', 
        bgColor: 'bg-gray-100',
        label: 'Chờ xử lý' 
      },
      resolved: { 
        icon: <CheckCircleIcon className="h-4 w-4" />, 
        color: 'text-green-600', 
        bgColor: 'bg-green-100',
        label: 'Đã giải quyết' 
      },
      closed: { 
        icon: <XCircleIcon className="h-4 w-4" />, 
        color: 'text-gray-600', 
        bgColor: 'bg-gray-100',
        label: 'Đã đóng' 
      },
      escalated: { 
        icon: <ExclamationTriangleIcon className="h-4 w-4" />, 
        color: 'text-red-600', 
        bgColor: 'bg-red-100',
        label: 'Đã leo thang' 
      }
    };
    return configs[status] || configs.new;
  };

  const getCategoryLabel = (category) => {
    const labels = {
      technical: 'Kỹ thuật',
      bug: 'Lỗi',
      feature_request: 'Yêu cầu tính năng',
      account: 'Tài khoản',
      billing: 'Thanh toán',
      general: 'Tổng quát'
    };
    return labels[category] || category;
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleString('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const handleAddComment = () => {
    if (newComment.trim()) {
      const comment = {
        id: comments.length + 1,
        author: 'Current User', // Thay bằng user hiện tại
        content: newComment,
        createdAt: new Date().toISOString(),
        type: 'internal'
      };
      setComments([...comments, comment]);
      setNewComment('');
    }
  };

  const renderStars = (stars) => {
    const starElements = [];
    for (let i = 1; i <= 5; i++) {
      starElements.push(
        <span key={i} className={`${i <= stars ? 'text-yellow-500' : 'text-gray-300'}`}>
          {i <= stars ? (
            <StarIconSolid className="h-4 w-4" />
          ) : (
            <StarIcon className="h-4 w-4" />
          )}
        </span>
      );
    }
    return starElements;
  };

  const getStarsLabel = (stars) => {
    const labels = {
      1: 'Bình thường',
      2: 'Quan trọng',
      3: 'Khẩn cấp',
      4: 'Rất khẩn cấp',
      5: 'Cực kỳ khẩn cấp'
    };
    return labels[stars] || 'Bình thường';
  };

  const priorityConfig = getPriorityConfig(ticket.priority);
  const statusConfig = getStatusConfig(ticket.status);

  return (
    <div className="fixed inset-0 bg-opacity-40 flex items-center justify-center z-50" style={{ backgroundColor: "rgba(0,0,0,0.5)" }}>
      <div className="bg-white rounded-lg shadow-xl max-w-4xl w-full mx-4 max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between">
          <div>
            <h2 className="text-xl font-semibold text-gray-900">
              Chi tiết Ticket - {ticket.id}
            </h2>
            <p className="text-sm text-gray-600 mt-1">
              {ticket.title}
            </p>
          </div>
          <div className="flex items-center space-x-2">
            <button
              onClick={() => {
                onClose();
                navigate(`/helpdesk/${ticket.id}`);
              }}
              className="text-indigo-600 hover:text-indigo-800 p-2 hover:bg-indigo-50 rounded-lg transition-colors"
              title="Chỉnh sửa"
            >
              <PencilIcon className="h-5 w-5" />
            </button>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600 p-2 hover:bg-gray-100 rounded-lg transition-colors"
            >
              <XMarkIcon className="h-6 w-6" />
            </button>
          </div>
        </div>

        <div className="p-6">
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Main Content */}
            <div className="lg:col-span-2 space-y-6">
              {/* Ticket Info */}
              <div className="bg-gray-50 rounded-lg p-4">
                <h3 className="text-lg font-medium text-gray-900 mb-3">Thông tin ticket</h3>
                <div className="space-y-3">
                  <div>
                    <label className="text-sm font-medium text-gray-700">Mô tả:</label>
                    <p className="text-sm text-gray-900 mt-1">{ticket.description}</p>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    <span className="text-sm font-medium text-gray-700">Tags:</span>
                    {ticket.tags?.map((tag, index) => (
                      <span
                        key={index}
                        className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-indigo-100 text-indigo-800"
                      >
                        {tag}
                      </span>
                    ))}
                  </div>
                </div>
              </div>

              {/* Status Actions */}
              <div className="bg-white border border-gray-200 rounded-lg p-4">
                <h3 className="text-lg font-medium text-gray-900 mb-3">Cập nhật trạng thái</h3>
                <div className="grid grid-cols-2 md:grid-cols-4 gap-2">
                  {['open', 'in_progress', 'pending', 'resolved'].map((status) => {
                    const config = getStatusConfig(status);
                    return (
                      <button
                        key={status}
                        onClick={() => onStatusChange(ticket.id, status)}
                        className={`flex items-center justify-center px-3 py-2 rounded-md text-sm font-medium transition-colors ${
                          ticket.status === status 
                            ? `${config.bgColor} ${config.color}` 
                            : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                        }`}
                      >
                        {config.icon}
                        <span className="ml-1">{config.label}</span>
                      </button>
                    );
                  })}
                </div>
              </div>

              {/* Comments Section */}
              <div className="bg-white border border-gray-200 rounded-lg p-4">
                <h3 className="text-lg font-medium text-gray-900 mb-4 flex items-center">
                  <ChatBubbleLeftRightIcon className="h-5 w-5 mr-2" />
                  Bình luận ({comments.length})
                </h3>
                
                {/* Comments List */}
                <div className="space-y-4 mb-4">
                  {comments.map((comment) => (
                    <div key={comment.id} className="bg-gray-50 rounded-lg p-3">
                      <div className="flex items-center justify-between mb-2">
                        <span className="font-medium text-sm text-gray-900">{comment.author}</span>
                        <span className="text-xs text-gray-500">{formatDate(comment.createdAt)}</span>
                      </div>
                      <p className="text-sm text-gray-700">{comment.content}</p>
                    </div>
                  ))}
                </div>

                {/* Add Comment */}
                <div className="border-t border-gray-200 pt-4">
                  <div className="flex space-x-3">
                    <div className="flex-1">
                      <textarea
                        value={newComment}
                        onChange={(e) => setNewComment(e.target.value)}
                        placeholder="Thêm bình luận..."
                        rows={2}
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
                      />
                    </div>
                    <button
                      onClick={handleAddComment}
                      disabled={!newComment.trim()}
                      className="px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 disabled:bg-gray-400 disabled:cursor-not-allowed"
                    >
                      Gửi
                    </button>
                  </div>
                </div>
              </div>
            </div>

            {/* Sidebar */}
            <div className="space-y-4">
              {/* Priority & Status */}
              <div className="bg-white border border-gray-200 rounded-lg p-4">
                <h3 className="text-sm font-medium text-gray-900 mb-3">Trạng thái & Ưu tiên</h3>
                <div className="space-y-3">
                  <div>
                    <label className="text-xs font-medium text-gray-500 uppercase tracking-wide">Trạng thái</label>
                    <div className={`mt-1 inline-flex items-center px-2.5 py-1.5 rounded-full text-xs font-medium ${statusConfig.bgColor} ${statusConfig.color}`}>
                      {statusConfig.icon}
                      <span className="ml-1">{statusConfig.label}</span>
                    </div>
                  </div>
                  <div>
                    <label className="text-xs font-medium text-gray-500 uppercase tracking-wide">Độ ưu tiên</label>
                    <div className={`mt-1 inline-flex items-center px-2.5 py-1.5 rounded-full text-xs font-medium ${priorityConfig.bgColor} ${priorityConfig.color}`}>
                      {priorityConfig.icon}
                      <span className="ml-1">{priorityConfig.label}</span>
                    </div>
                  </div>
                  <div>
                    <label className="text-xs font-medium text-gray-500 uppercase tracking-wide">Danh mục</label>
                    <p className="mt-1 text-sm text-gray-900">{getCategoryLabel(ticket.category)}</p>
                  </div>
                  <div>
                    <label className="text-xs font-medium text-gray-500 uppercase tracking-wide">Mức độ khẩn cấp</label>
                    <div className="mt-1 flex items-center space-x-1">
                      <div className="flex items-center space-x-1">
                        {renderStars(ticket.stars || 1)}
                      </div>
                      <span className="text-xs text-gray-600 ml-2">
                        {ticket.stars || 1} sao - {getStarsLabel(ticket.stars || 1)}
                      </span>
                    </div>
                  </div>
                  {ticket.slaBreached && (
                    <div className="mt-2 p-2 bg-red-50 border border-red-200 rounded-md">
                      <p className="text-xs text-red-800 font-medium">⚠️ SLA đã bị vi phạm</p>
                    </div>
                  )}
                </div>
              </div>

              {/* Customer Info */}
              <div className="bg-white border border-gray-200 rounded-lg p-4">
                <h3 className="text-sm font-medium text-gray-900 mb-3 flex items-center">
                  <UserIcon className="h-4 w-4 mr-2" />
                  Thông tin khách hàng
                </h3>
                <div className="space-y-2">
                  <div>
                    <label className="text-xs font-medium text-gray-500">Tên</label>
                    <p className="text-sm text-gray-900">{ticket.customer.name}</p>
                  </div>
                  <div>
                    <label className="text-xs font-medium text-gray-500">Email</label>
                    <p className="text-sm text-gray-900">{ticket.customer.email}</p>
                  </div>
                  <div>
                    <label className="text-xs font-medium text-gray-500">Điện thoại</label>
                    <p className="text-sm text-gray-900">{ticket.customer.phone}</p>
                  </div>
                </div>
              </div>

              {/* Assignment */}
              <div className="bg-white border border-gray-200 rounded-lg p-4">
                <h3 className="text-sm font-medium text-gray-900 mb-3">Phân công</h3>
                <div className="space-y-2">
                  <div>
                    <label className="text-xs font-medium text-gray-500">Được giao cho</label>
                    <p className="text-sm text-gray-900">
                      {ticket.assignedTo ? ticket.assignedTo.name : 'Chưa phân công'}
                    </p>
                  </div>
                </div>
              </div>

              {/* Timeline */}
              <div className="bg-white border border-gray-200 rounded-lg p-4">
                <h3 className="text-sm font-medium text-gray-900 mb-3 flex items-center">
                  <CalendarIcon className="h-4 w-4 mr-2" />
                  Thời gian
                </h3>
                <div className="space-y-2">
                  <div>
                    <label className="text-xs font-medium text-gray-500">Tạo lúc</label>
                    <p className="text-sm text-gray-900">{formatDate(ticket.createdAt)}</p>
                  </div>
                  <div>
                    <label className="text-xs font-medium text-gray-500">Cập nhật lần cuối</label>
                    <p className="text-sm text-gray-900">{formatDate(ticket.updatedAt)}</p>
                  </div>
                  {ticket.dueDate && (
                    <div>
                      <label className="text-xs font-medium text-gray-500">Hạn xử lý</label>
                      <p className="text-sm text-gray-900">{formatDate(ticket.dueDate)}</p>
                    </div>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default TicketDetailModal;