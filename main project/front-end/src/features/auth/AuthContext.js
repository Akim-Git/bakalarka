import React, { createContext, useContext, useState, useEffect } from 'react';
import axios from 'axios';

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    

    useEffect(() => {
    // Check user authentication on page load
    axios.get('https://localhost:7006/api/auth/validate', { withCredentials: true })
        .then(response => {
            if (response.data.isAuthenticated) {
                console.log("User is authenticated:", response.data.user);
                setUser(response.data.user);  // Set user from backend response
                setIsAuthenticated(true);
            } else {
                console.log("AAAAAAAAAAAAAA")
                console.log("User is not authenticated.");
                logout(); // Log out if not authenticated
            }
        })
        .catch(error => {
            console.log("Error validating user:", error.response ? error.response.data : error.message);
            console.log("BBBBBBBBBBBBBB")
            logout(); // Log out on error
        });
}, []);


    const login = (userData) => {
        setUser(userData);
        setIsAuthenticated(true);
    };

    const logout = async () => {
        try {
            await axios.post('https://localhost:7006/api/auth/logout', {}, { withCredentials: true });
        } catch (error) {
            console.log("Error during logout:", error);
        }finally {
            setIsAuthenticated(false);            
        }
    };

    return (
        <AuthContext.Provider value={{ user, isAuthenticated, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    return useContext(AuthContext);
};
