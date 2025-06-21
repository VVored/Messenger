import React, { useEffect, useState } from 'react';
import style from './Login.module.css'
import { useNavigate } from 'react-router-dom';
import FilePicker from './FilePicker';

const RegisterPage = () => {
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [error, setError] = useState('');
    const [avatarUrl, setAvatarUrl] = useState('');
    const navigate = useNavigate()

    const registerUser = async () => {
        if (!username || !password || !email || !firstName || !lastName || !avatarUrl) {
            setError('Заполните все поля');
            return;
        }
        try {
            const response = await fetch('http://45.144.222.67:5266/api/users/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username, email, password, firstName, lastName, avatarUrl }),
            });
            if (response.ok) {
                navigate('/login'); 
            }
        } catch (err) {
            setError('Ошибка');
        }
    }    

    useEffect(() => {
        console.log(avatarUrl);
    }, [avatarUrl])

    return (
        <div className={style.container}>
            <FilePicker setAvatarUrl={setAvatarUrl}></FilePicker>
            <input className={style.input} type="text" value={username} onChange={(e) => setUsername(e.target.value)} placeholder="Имя пользователя" />
            <input className={style.input} type="text" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Электронная почта" />
            <input className={style.input} type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Пароль" />
            <input className={style.input} type="text" value={firstName} onChange={(e) => setFirstName(e.target.value)} placeholder="Имя" />
            <input className={style.input} type="text" value={lastName} onChange={(e) => setLastName(e.target.value)} placeholder="Фамилия" />
            <button className={style.button} onClick={registerUser}>Зарегистрировать аккаунт</button>
            {error ?? <p className={style.error}>{error}</p>}
        </div>
    );
};

export default RegisterPage;