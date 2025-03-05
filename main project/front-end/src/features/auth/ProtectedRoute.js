import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from './AuthContext';

const ProtectedRoute = ({ children }) => {
    const { isAuthenticated } = useAuth();
    
    console.log("Is authenticated?", isAuthenticated); 

   
    return isAuthenticated ? children : <Navigate to="/" />;
};

export default ProtectedRoute;
