import React, { useState } from 'react';
import style from './Login.module.css'
import { useNavigate } from 'react-router-dom';

const RegisterPage = () => {
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [error, setError] = useState('');
    const navigate = useNavigate()

    const registerUser = async () => {
        if (!username || !password || !email || !firstName || !lastName) {
            setError('Please fill in all fields');
            return;
        }
        try {
            const response = await fetch('https://localhost:7192/api/users/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, email, password, firstName, lastName }),
            });
            if (response.ok) {
                navigate('/login'); 
            }
        } catch (err) {
            setError(err);
        }
    }    

    return (
        <div className={style.container}>
            <input className={style.input} type="text" value={username} onChange={(e) => setUsername(e.target.value)} placeholder="Username" />
            <input className={style.input} type="text" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Email" />
            <input className={style.input} type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Password" />
            <input className={style.input} type="text" value={firstName} onChange={(e) => setFirstName(e.target.value)} placeholder="First name" />
            <input className={style.input} type="text" value={lastName} onChange={(e) => setLastName(e.target.value)} placeholder="Last name" />
            <button className={style.button} onClick={registerUser}>Register user</button>
            {error ?? <p className={style.error}>{error}</p>}
        </div>
    );
};

export default RegisterPage;