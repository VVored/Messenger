import React, { useState } from 'react';
import style from './Login.module.css'
import { Link, useNavigate } from 'react-router-dom';

const Login = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const navigate = useNavigate()

    const handleLogin = async () => {
        if (!username || !password) {
            setError('Заполните все поля');
            return;
        }
        try {
            const response = await fetch('https://localhost:7192/api/authorization/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, password }),
            });
            const data = await response.json();
            if (data.token) {
                localStorage.setItem('token', data.token);
                navigate('/chats'); 
            }
        } catch (err) {
            setError('Неверное имя пользователя или пароль')
        }
        
    };

    return (
        <div className={style.container}>
            <input className={style.input} type="text" value={username} onChange={(e) => setUsername(e.target.value)} placeholder="Имя пользователя" />
            <input className={style.input} type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Пароль" />
            <button className={style.button} onClick={handleLogin}>Войти</button>
            {error ?? <p className={style.error}>{error}</p>}
            <Link to="/register" className={style.register_link}>Зарегистрироваться</Link>            
        </div>
    );
};

export default Login;