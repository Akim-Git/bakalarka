import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom'; // Přidáno useNavigate
import axios from 'axios';
import './LobbyMenuStyle.css';

const LobbyMenu = () => {
    const [lobbies, setLobbies] = useState([]);
    const navigate = useNavigate(); // Inicializace navigate

    const fetchLobbies = async () => {
        try {
            const response = await axios.get('https://localhost:7006/api/Lobby/get-lobbies', {
                withCredentials: true
            });
            setLobbies(response.data.lobbies);
        } catch (error) {
            console.error('Chyba v obdržení dat:', error);
        }
    };

    // Vyvolá funkci během načítání
    useEffect(() => {
        fetchLobbies();
    }, []);

    const handleNavigateToLobby = (id) => {
        navigate(`/quiz-builder-multiplayer/${id}`);
        console.log(`lobby id "${id}"`);
    };

    return (
        <div className="lobby-grid">
            {lobbies.map((lobby, index) => (
                <div key={index} className="lobby-item">
                    <p><strong>Lobby Name:</strong> {lobby.name}</p>
                    <span style={{ display: "none" }}>{lobby.id}</span>
                    <button onClick={() => handleNavigateToLobby(lobby.id)}>Join</button>
                </div>
            ))}
        </div>
    );
};

export default LobbyMenu;
