import React, { useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

const Register = () => {
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState(''); 
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [isLoading, setIsLoading] = useState(false);  
    const navigate = useNavigate();
    const [showPassword, setShowPassword] = useState(false);

    const [usernameError, setUsernameError] = useState('');
    const [emailError, setEmailError] = useState('');
    const [passwordError, setPasswordError] = useState('');

    const handleRegister = async (e) => {
        e.preventDefault();
        
        if (password !== confirmPassword) {
            setPasswordError('Hesla se neshodují'); 
            return;
        }

        setIsLoading(true); 
        setUsernameError('');
        setEmailError('');
        setPasswordError('');

        try {
            await axios.post('https://localhost:7006/api/auth/register', {
                username,
                email,
                password,
            });
            alert('Registrace byla úspěšná! Musíte potvrdit email a můžete se přihlásit.');
            navigate('/');
        } catch (error) {
            if (error.response && error.response.data) {
                const { data } = error.response;
                // Zkontrolujeme přesnou chybovou zprávu z backendu a nastavíme příslušnou chybu
                if (data === "Tento e-mail je již používán.") {
                    setEmailError(data);
                } else if (data === "Toto uživatelské jméno je již používáno.") {
                    setUsernameError(data);
                } else if (data === "Tento e-mail a uživatelské jméno jsou již používány.") {
                    setEmailError("Tento e-mail je již používán.");
                    setUsernameError("Toto uživatelské jméno je již používáno.");
                } else {
                    setPasswordError("Registrace selhala: " + data);
                }
            } else {
                setPasswordError("Registrace selhala: " + error.message);
            }
        } finally {
            setIsLoading(false); 
        }
    };

    return (
        <div>
            <h2>Registrace</h2>
            <form onSubmit={handleRegister}>
                <div style={{ marginBottom: '15px' }}>
                    <input 
                        type="text" 
                        placeholder="Uživatelské jméno" 
                        value={username}
                        onChange={(e) => setUsername(e.target.value)} 
                        required 
                    />
                    {usernameError && <p style={{ color: 'red', fontSize: '0.9em' }}>{usernameError}</p>}
                </div>
                
                <div style={{ marginBottom: '15px' }}>
                    <input 
                        type="email" 
                        placeholder="E-mail" 
                        value={email}
                        onChange={(e) => setEmail(e.target.value)} 
                        required 
                    />
                    {emailError && <p style={{ color: 'red', fontSize: '0.9em' }}>{emailError}</p>}
                </div>

                <div style={{ marginBottom: '15px' }}>
                    <input 
                        type={showPassword ? "text" : "password"}
                        placeholder="Heslo" 
                        value={password}
                        onChange={(e) => setPassword(e.target.value)} 
                        required 
                    />
                </div>
                
                <div style={{ marginBottom: '15px' }}>
                    <input 
                        type={showPassword ? "text" : "password"}
                        placeholder="Potvrďte heslo" 
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)} 
                        required 
                    />
                    {passwordError && <p style={{ color: 'red', fontSize: '0.9em' }}>{passwordError}</p>}
                </div>
                
                <label>
                    <span>Zobrazit heslo</span>
                    <input
                        type='checkbox'
                        checked={showPassword}
                        onChange={() => setShowPassword(!showPassword)}
                    />
                </label>

                <button type="submit" disabled={isLoading}> 
                    {isLoading ? 'Registruji...' : 'Registrovat'}
                </button>
            </form>
            <p>
                Již máte účet? <a href="/">Přihlaste se zde</a>
            </p>
        </div>
    );
};

export default Register;
