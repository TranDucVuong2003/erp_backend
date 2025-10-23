import React from 'react';
import { useNavigate } from 'react-router-dom';
import TicketForm from './TicketForm';

const TicketCreatePage = () => {
  const navigate = useNavigate();

  const handleSubmit = (ticketData) => {
    // Here you would typically send the data to your API
    console.log('Creating new ticket:', ticketData);
    
    // After successful creation, navigate back to helpdesk
    navigate('/helpdesk');
  };

  const handleClose = () => {
    navigate('/helpdesk');
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <TicketForm
        isOpen={true}
        onClose={handleClose}
        ticket={null}
        onSubmit={handleSubmit}
      />
    </div>
  );
};

export default TicketCreatePage;
