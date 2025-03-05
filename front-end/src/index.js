import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter as Router } from 'react-router-dom';
import App from './App';
import { AuthProvider } from './features/auth/AuthContext'; // Import AuthProvider
import reportWebVitals from './reportWebVitals'; // Import reportWebVitals

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
    <AuthProvider>
        <Router>
            <App />
        </Router>
    </AuthProvider>
);


reportWebVitals(console.log); // or remove the call if not needed




// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals

