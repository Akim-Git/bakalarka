import React, { useEffect, useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import { useAuth } from './AuthContext';

const Login = () => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const [forgotPasswordEmail, setForgotPasswordEmail] = useState('');
    const [isForgotPassword, setIsForgotPassword] = useState(false);
    const navigate = useNavigate();
    const { user, isAuthenticated, login, token } = useAuth();

    const [showPassword, setShowPassword] = useState(false);

    useEffect(() => {
        console.log("isAuthenticated on component mount:", isAuthenticated);
        if (isAuthenticated) {
            navigate('/home');
        }
    }, [user, navigate, isAuthenticated]);

    const handleLogin = async (e) => {
        e.preventDefault();
        setIsLoading(true);
        try {
            const response = await axios.post('https://localhost:7006/api/auth/login', {
                email,
                password,
            }, { withCredentials: true });

            console.log("Login response:", response.data);
            login(response.data.user);
        } catch (error) {
            console.error('Login error:', error);
            alert('Login failed: ' + (error.response ? error.response.data.message : error.message));
        } finally {
            setIsLoading(false);
        }
    };

    const handleForgotPassword = async (e) => {
        e.preventDefault();
        try {
            await axios.post('https://localhost:7006/api/auth/forgot-password', {
                email: forgotPasswordEmail,
            });
            alert('Password reset link has been sent to your email.');
            setIsForgotPassword(false);
        } catch (error) {
            console.error('Forgot password error:', error);
            alert('Failed to send password reset email: ' + (error.response ? error.response.data.message : error.message));
        }
    };

    return (
        <div>
            <h2>{isForgotPassword ? 'Forgot Password' : 'Login'}</h2>
            {isForgotPassword ? (
                <form onSubmit={handleForgotPassword}>
                    <input
                        type="email"
                        placeholder="Email"
                        value={forgotPasswordEmail}
                        onChange={(e) => setForgotPasswordEmail(e.target.value)}
                        required
                    />
                    <button type="submit" disabled={isLoading}>
                        {isLoading ? 'Sending...' : 'Send Reset Link'}
                    </button>
                    <p onClick={() => setIsForgotPassword(false)} style={{ cursor: 'pointer', color: 'blue' }}>
                        Back to Login
                    </p>
                </form>
            ) : (
                <form onSubmit={handleLogin}>
                    <input
                        type="text"
                        placeholder="Email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                    />
                    <input
                        type={showPassword ? "text" : "password"}
                        placeholder="Password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                    <label>
                        <span>Show password</span>
                        <input
                            type='checkbox'
                            checked={showPassword}
                            onChange={()=> setShowPassword(!showPassword)}
                        />
                    </label>
                    <button type="submit" disabled={isLoading}>
                        {isLoading ? 'Logging in...' : 'Login'}
                    </button>
                    <p onClick={() => setIsForgotPassword(true)} style={{ cursor: 'pointer', color: 'blue' }}>
                        Forgot your password?
                    </p>
                    <p>
                        Don't have an account? <a href="/register">Register here</a>
                    </p>
                </form>
            )}
            {token && <p>Your JWT Token: {token}</p>}
        </div>
    );
};

export default Login;
