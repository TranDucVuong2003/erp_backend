import React, { useEffect, useState } from 'react'
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
  EyeIcon
} from '@heroicons/react/24/outline';

const ViewAllTicketsModal = ({ 
  isOpen, 
  onClose, 
  tickets, 
  defaultPriorityFilter,
  statuses,
  priorities,
  categories,
  agents,
  onStatusChange,
  onAssignTicket,
  onPriorityChange,
  onViewTicket,
  onEditTicket
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState('all');
  const [priorityFilter, setPriorityFilter] = useState(defaultPriorityFilter);
  const [categoryFilter, setCategoryFilter] = useState('all');
  const [assignedToFilter, setAssignedToFilter] = useState('all');
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(10);

  // Reset filters when modal opens with new default priority
  useEffect(() => {
    setPriorityFilter(defaultPriorityFilter);
    setCurrentPage(1);
  }, [defaultPriorityFilter, isOpen]);

  // Filter tickets
  const filteredTickets = tickets.filter(ticket => {
    const matchesSearch = 
      ticket.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
      ticket.id.toLowerCase().includes(searchTerm.toLowerCase()) ||
      ticket.customer.name.toLowerCase().includes(searchTerm.toLowerCase());

    const matchesStatus = statusFilter === 'all' || ticket.status === statusFilter;
    const matchesPriority = priorityFilter === 'all' || ticket.priority === priorityFilter;
    const matchesCategory = categoryFilter === 'all' || ticket.category === categoryFilter;
    const matchesAssignee = assignedToFilter === 'all' || ticket.assignedTo?.id === parseInt(assignedToFilter);

    return matchesSearch && matchesStatus && matchesPriority && matchesCategory && matchesAssignee;
  });

  // Pagination
  const totalItems = filteredTickets.length;
  const totalPages = Math.ceil(totalItems / itemsPerPage);
  const startIndex = (currentPage - 1) * itemsPerPage;
  const currentPageTickets = filteredTickets.slice(startIndex, startIndex + itemsPerPage);

  const getStatusInfo = (status) => statuses.find(s => s.id === status) || statuses[0];
  const getPriorityInfo = (priority) => priorities.find(p => p.id === priority) || priorities[0];
  const getCategoryInfo = (category) => categories.find(c => c.id === category) || categories[0];

  const formatDate = (dateString) => new Date(dateString).toLocaleString('vi-VN');

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      <div className="flex min-h-screen items-center justify-center p-4">
        <div className="fixed inset-0 bg-opacity-50" onClick={onClose} style={{ backgroundColor: "rgba(0,0,0,0.5)" }} />
        
        <div className="relative bg-white rounded-lg shadow-xl max-w-7xl w-full h-[800px] flex flex-col">
          {/* Modal Header */}
          <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between flex-shrink-0">
            <div>
              <h2 className="text-xl font-semibold text-gray-900">All Tickets</h2>
              <p className="text-sm text-gray-600">
                {priorityFilter !== 'all' && `Filtered by ${priorities.find(p => p.id === priorityFilter)?.name} priority - `}
                {totalItems} tickets found
              </p>
            </div>
            <button
              onClick={onClose}
              className="text-gray-400 hover:text-gray-600 transition-colors"
            >
              <XMarkIcon className="h-6 w-6" />
            </button>
          </div>

          {/* Search and Filters */}
          <div className="px-6 py-4 border-b border-gray-200 bg-gray-50 flex-shrink-0">
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-6 gap-4">
              {/* Search */}
              <div className="lg:col-span-2">
                <div className="relative">
                  <SearchIcon className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
                  <input
                    type="text"
                    placeholder="Search tickets..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="w-full pl-10 pr-4 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
                  />
                </div>
              </div>

              {/* Status Filter */}
              <div>
                <select
                  value={statusFilter}
                  onChange={(e) => setStatusFilter(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
                >
                  <option value="all">All Status</option>
                  {statuses.map(status => (
                    <option key={status.id} value={status.id}>{status.name}</option>
                  ))}
                </select>
              </div>

              {/* Priority Filter */}
              <div>
                <select
                  value={priorityFilter}
                  onChange={(e) => setPriorityFilter(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
                >
                  <option value="all">All Priority</option>
                  {priorities.map(priority => (
                    <option key={priority.id} value={priority.id}>{priority.name}</option>
                  ))}
                </select>
              </div>

              {/* Category Filter */}
              <div>
                <select
                  value={categoryFilter}
                  onChange={(e) => setCategoryFilter(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
                >
                  <option value="all">All Category</option>
                  {categories.map(category => (
                    <option key={category.id} value={category.id}>{category.name}</option>
                  ))}
                </select>
              </div>

              {/* Assignee Filter */}
              <div>
                <select
                  value={assignedToFilter}
                  onChange={(e) => setAssignedToFilter(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"
                >
                  <option value="all">All Assignee</option>
                  {agents.map(agent => (
                    <option key={agent.id} value={agent.id}>{agent.name}</option>
                  ))}
                </select>
              </div>
            </div>
          </div>

          {/* Tickets List */}
          <div className="flex-1 overflow-y-auto">
            <div className="px-6 py-4">
              {currentPageTickets.length > 0 ? (
                <div className="space-y-4">
                  {currentPageTickets.map((ticket) => {
                    const statusInfo = getStatusInfo(ticket.status);
                    const priorityInfo = getPriorityInfo(ticket.priority);
                    const categoryInfo = getCategoryInfo(ticket.category);

                    return (
                      <div key={ticket.id} className="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow">
                        <div className="flex items-start justify-between">
                          <div className="flex-1">
                            <div className="flex items-center space-x-3 mb-2">
                              <span className="text-sm font-mono text-gray-500">{ticket.id}</span>
                              <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${priorityInfo.color}`}>
                                {priorityInfo.name}
                              </span>
                              <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${statusInfo.color}`}>
                                {statusInfo.icon} {statusInfo.name}
                              </span>
                              <span className={`inline-flex items-center px-2 py-1 rounded-full text-xs font-medium ${categoryInfo.color || 'bg-gray-100 text-gray-800'}`}>
                                {categoryInfo.icon} {categoryInfo.name}
                              </span>
                            </div>
                            
                            <h3 className="text-sm font-semibold text-gray-900 mb-1">{ticket.title}</h3>
                            <p className="text-sm text-gray-600 line-clamp-2 mb-2">{ticket.description}</p>
                            
                            <div className="flex items-center space-x-4 text-xs text-gray-500">
                              <span>Customer: {ticket.customer.name}</span>
                              <span>Assigned: {ticket.assignedTo?.name || 'Unassigned'}</span>
                              <span>Created: {formatDate(ticket.createdAt)}</span>
                            </div>
                          </div>
                          
                          <div className="flex items-center space-x-2 ml-4">
                            <button
                              onClick={() => onViewTicket(ticket)}
                              className="p-2 text-gray-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-md transition-colors"
                              title="View Details"
                            >
                              <EyeIcon className="h-4 w-4" />
                            </button>
                            <select
                              value={ticket.status}
                              onChange={(e) => onStatusChange(ticket.id, e.target.value)}
                              className="text-xs border border-gray-300 rounded px-2 py-1 focus:ring-1 focus:ring-indigo-500"
                            >
                              {statuses.map(status => (
                                <option key={status.id} value={status.id}>{status.name}</option>
                              ))}
                            </select>
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              ) : (
                <div className="text-center py-12">
                  <p className="text-gray-500">No tickets found matching your criteria.</p>
                </div>
              )}
            </div>
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="px-6 py-4 border-t border-gray-200 bg-gray-50 flex-shrink-0">
              <div className="flex items-center justify-between">
                <div className="text-sm text-gray-700">
                  Showing {startIndex + 1} to {Math.min(startIndex + itemsPerPage, totalItems)} of {totalItems} results
                </div>
                <div className="flex space-x-2">
                  <button
                    onClick={() => setCurrentPage(1)}
                    disabled={currentPage === 1}
                    className="px-3 py-1 text-sm font-medium rounded-md bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-50 transition-opacity"
                  >
                    First
                  </button>
                  <button
                    onClick={() => setCurrentPage(currentPage - 1)}
                    disabled={currentPage === 1}
                    className="px-3 py-1 text-sm font-medium rounded-md bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-50 transition-opacity"
                  >
                    Previous
                  </button>
                  <button
                    onClick={() => setCurrentPage(currentPage + 1)}
                    disabled={currentPage === totalPages}
                    className="px-3 py-1 text-sm font-medium rounded-md bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-50 transition-opacity"
                  >
                    Next
                  </button>
                  <button
                    onClick={() => setCurrentPage(totalPages)}
                    disabled={currentPage === totalPages}
                    className="px-3 py-1 text-sm font-medium rounded-md bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-50 transition-opacity"
                  >
                    Last
                  </button>
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};  

export default ViewAllTicketsModal
