import React from 'react';
import { Route, Routes } from 'react-router-dom';
import Login from './features/auth/Login';
import Register from './features/auth/Register';
import CreateQuiz from './features/quiz/CreateQuiz';
import Home from './features/home/Home';
import ProtectedRoute from './features/auth/ProtectedRoute'; 
import { AuthProvider } from './features/auth/AuthContext'; 
import ResetPassword from './features/auth/ResetPassword'; 
import QuizBuilder from './features/quizBuilder/QuizBuilder';
import LobbyMenu from './features/lobby/LobbyMenu';
import QuizBuilderMulti from './features/quizBuilder/QuizBuilderMulti';

const App = () => {
    return (
        <AuthProvider> 
            <div className="App">
                <h1>Moje Kvízová Aplikace</h1>
                <Routes>
                    <Route path="/" element={<Login />} />
                    <Route path="/register" element={<Register />} />
                    <Route path="/resetpassword" element={<ResetPassword />} /> {/* Přidejte tuto řádku */}
                    <Route
                        path="/create-quiz"
                        element={
                            <ProtectedRoute>
                                <CreateQuiz />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/home"
                        element={
                            <ProtectedRoute>
                                <Home />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/quiz-builder/:id"
                        element={
                            <ProtectedRoute>
                                <QuizBuilder />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path='/lobby-menu'
                        element={
                            <ProtectedRoute>
                                <LobbyMenu />
                            </ProtectedRoute>
                        }
                    
                    />
                    <Route
                        path='/quiz-builder-multiplayer/:id'
                        element={
                            <ProtectedRoute>
                                <QuizBuilderMulti />
                            </ProtectedRoute>
                        }
                    />
                </Routes>
            </div>
        </AuthProvider>
    );
};

export default App;
