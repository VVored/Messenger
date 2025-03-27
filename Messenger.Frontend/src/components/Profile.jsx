import React, { useEffect, useState } from 'react'
import { jwtDecode } from 'jwt-decode';

const Profile = ({ selectedUser, setSelectedUser }) => {

    const [isSelectedUserAuthorizeNow, setIsSelectedUserAuthorizeNow] = useState(false);

    useEffect(() => {
        setIsSelectedUserAuthorizeNow(false);
        const token = localStorage.getItem('token');
        const decodedToken = jwtDecode(token);
        if (decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] === selectedUser.userId + '') {
            setIsSelectedUserAuthorizeNow(true);
        }
    }, [selectedUser])

    return (
        <div style={{ width: "30vw", overflowY: "auto", minHeight: "100vh" }}>
            <div style={{ backgroundColor: '#cccccc', height: '33%', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', margin: '0' }}>
                <button onClick={() => { setSelectedUser(null) }} style={{ marginRight: 'auto', border: 'none', fontSize: '30px', color: 'white', backgroundColor: '#cccccc', cursor: 'pointer' }}>×</button>
                <img width='40%' style={{ border: '0px solid black', borderRadius: '100%' }} src={`https://localhost:7192/api/files/${selectedUser.avatarUrl}`} alt={`${selectedUser.avatarUrl}`}></img>
                <h3 style={{ color: 'white' }}>{selectedUser.firstName} {selectedUser.lastName}</h3>
            </div>
            {
                isSelectedUserAuthorizeNow
                    ? <button style={{ backgroundColor: 'rgba(178, 178, 178)', border: 'none', width: '100%', cursor: 'pointer', fontWeight: '700', color: 'white', padding: '2%' }}>Отредактировать профиль</button>
                    : <button style={{ backgroundColor: 'rgba(178, 178, 178)', border: 'none', width: '100%', cursor: 'pointer', fontWeight: '700', color: 'white', padding: '2%' }}>Отправить сообщение</button>

            }
            <p>Имя пользователя: <b>@{`${selectedUser.username}`}</b></p>
        </div>
    )
}

export default Profile;