import React, { useEffect, useState } from 'react';
import style from './Login.module.css'
import { useNavigate, useParams } from 'react-router-dom';
import FilePicker from './FilePicker';
import axios from 'axios';
import { jwtDecode } from 'jwt-decode';

const EditUser = () => {
    const params = useParams();
    const id = params.id;
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [firstName, setFirstName] = useState('');
    const [lastName, setLastName] = useState('');
    const [error, setError] = useState('');
    const [avatarUrl, setAvatarUrl] = useState('');
    const navigate = useNavigate();

    const fetchData = async () => {
        const decoded = jwtDecode(localStorage.getItem('token'));
        if (decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] !== id) {
            navigate('/chats');
        } else {
            const response = await axios.get(`http://45.144.222.67:5266/api/users/${id}`);
            if (response.status === 200) {
                setUsername(response.data.username);
                setAvatarUrl(response.data.avatarUrl);
                setEmail(response.data.email);
                setFirstName(response.data.firstName);
                setLastName(response.data.lastName);
            }
        }
    }

    const editUser = async () => {
        if (!username || !email || !firstName || !lastName || !avatarUrl) {
            setError('Заполните все поля');
            return;
        }
        try {
            const response = await axios.put(`http://45.144.222.67:5266/api/users/${id}`, {id, username, email, password, firstName, lastName, avatarUrl})
            if (response.status === 200) {
                navigate('/chats');
            }
        } catch (err) {
            setError('Ошибка');
        }
    }

    useEffect(() => {
        fetchData();
    }, [])

    return (
        <div className={style.container}>
            <FilePicker setAvatarUrl={setAvatarUrl}></FilePicker>
            <input className={style.input} type="text" value={username} onChange={(e) => setUsername(e.target.value)} placeholder="Имя пользователя" />
            <input className={style.input} type="text" value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Электронная почта" />
            <input className={style.input} type="password" value={password} onChange={(e) => setPassword(e.target.value)} placeholder="Пароль" />
            <input className={style.input} type="text" value={firstName} onChange={(e) => setFirstName(e.target.value)} placeholder="Имя" />
            <input className={style.input} type="text" value={lastName} onChange={(e) => setLastName(e.target.value)} placeholder="Фамилия" />
            <button className={style.button} onClick={editUser}>Отредактировать</button>
            {error ?? <p className={style.error}>{error}</p>}
        </div>
    );
};

export default EditUser;