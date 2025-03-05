import React, { useState } from 'react';
import { useLocation } from 'react-router-dom';
import { useNavigate } from 'react-router-dom';

const ResetPassword = () => {
    const location = useLocation();
    const query = new URLSearchParams(location.search);
    const token = query.get('token');
    const email = query.get('email');

    const [newPassword, setNewPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');

    const [showPassword, setShowPassword] = useState(false);

    const navigate = useNavigate();

    const handleResetPassword = async () => {
        const dataToSend = {
            email,
            resetCode: token,
            newPassword
        };
    
        // Zobraz data, která budou odeslána na server, ve formátu JSON
        console.log("Data to send:", JSON.stringify(dataToSend, null, 2));
    
        const response = await fetch('https://localhost:7006/api/Auth/reset-password', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dataToSend),
        });
    
        if (response.ok) {
            alert('Heslo bylo úspěšně resetováno!');
            navigate('/home');
        } else {
            alert('Nastala chyba při resetování hesla.');
        }
    };

    return (
        <div>
            <h2>Resetování Hesla</h2>
            <input
                type={showPassword ? "text" : "password"}
                placeholder="Nové heslo"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
            />
            <input
                type={showPassword ? "text" : "password"}
                placeholder="Potvrdit nové heslo"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
            />
            <label>
                <span>Show password</span>
                <input
                    type='checkbox'
                    checked={showPassword}
                    onChange={()=> setShowPassword(!showPassword)}
                
                />
            </label>
            <button onClick={handleResetPassword}>Obnovit heslo</button>
        </div>
    );
};

export default ResetPassword;
