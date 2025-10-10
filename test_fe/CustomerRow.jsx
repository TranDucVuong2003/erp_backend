import React from 'react'
import { 
  PencilIcon, 
  TrashIcon,
  EyeIcon,
  PhoneIcon,
  EnvelopeIcon as MailIcon
} from '@heroicons/react/24/outline';

const CustomerRow = ({ customer, onEdit, onDelete, onView }) => {
  const getStatusColor = (status) => {
    switch (status) {
      case 'active': return 'bg-green-100 text-green-800';
      case 'inactive': return 'bg-red-100 text-red-800';
      case 'potential': return 'bg-yellow-100 text-yellow-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const getStatusText = (status) => {
    switch (status) {
      case 'active': return 'Hoạt động';
      case 'inactive': return 'Không hoạt động';
      case 'potential': return 'Tiềm năng';
      default: return status;
    }
  };

  // Get the display name - try fullName first, then name, then fallback
  const displayName = customer.fullName || customer.name || 'N/A';
  const nameInitial = displayName !== 'N/A' ? displayName.charAt(0).toUpperCase() : '?';

  return (
    <tr className="hover:bg-gray-50">
      <td className="px-6 py-4 whitespace-nowrap">
        <div className="flex items-center">
          <div className="h-10 w-10 flex-shrink-0">
            <div className="h-10 w-10 rounded-full bg-indigo-500 flex items-center justify-center">
              <span className="text-sm font-medium text-white">
                {nameInitial}
              </span>
            </div>
          </div>
          <div className="ml-4">
            <div className="text-sm font-medium text-gray-900">{displayName}</div>
            <div className="text-sm text-gray-500">{customer.company || ''}</div>
          </div>
        </div>
      </td>
      <td className="px-6 py-4 whitespace-nowrap">
        <div className="flex items-center text-sm text-gray-900">
          <PhoneIcon className="h-4 w-4 mr-2 text-gray-400" />
          {customer.phoneNumber || 'N/A'}
        </div>
        <div className="flex items-center text-sm text-gray-500 mt-1">
          <MailIcon className="h-4 w-4 mr-2 text-gray-400" />
          {customer.email || 'N/A'}
        </div>
      </td>
      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
        {customer.address || 'N/A'}
      </td>
      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
        <div className="flex flex-col">
          <span className="font-medium">
            {(customer.source === 'Individual') ? 'Cá nhân' : 'Công ty'}
          </span>
          {customer.source === 'Individual' && customer.referrer && (
            <span className="text-xs text-gray-500 mt-1">
              Giới thiệu: {customer.referrer}
            </span>
          )}
        </div>
      </td>
      <td className="px-6 py-4 text-sm text-gray-900 max-w-xs">
        <div className="truncate" title={customer.notes || ''}>
          {customer.notes || '-'}
        </div>
      </td>
      <td className="px-6 py-4 whitespace-nowrap">
        <span className={`inline-flex px-2 py-1 text-xs font-semibold rounded-full ${getStatusColor(customer.status)}`}>
          {getStatusText(customer.status)}
        </span>
      </td>
      <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
        <div className="flex justify-end space-x-2">
          <button
            onClick={() => onView(customer)}
            className="text-indigo-600 hover:text-indigo-900"
          >
            <EyeIcon className="h-4 w-4" />
          </button>
          <button
            onClick={() => onEdit(customer)}
            className="text-indigo-600 hover:text-indigo-900"
          >
            <PencilIcon className="h-4 w-4" />
          </button>
          <button
            onClick={() => onDelete(customer.id)}
            className="text-red-600 hover:text-red-900"
          >
            <TrashIcon className="h-4 w-4" />
          </button>
        </div>
      </td>
    </tr>
  );
};

export default CustomerRow;