import React from 'react'
import style from './Chat.module.css';

const SearchedUser = ({ user, onClick }) => {

    return (
        <div className={style.chat_container} onClick={onClick}>
            <img className={style.chat_avatar} src={`http://45.144.222.67:5266/api/files/${user.avatarUrl}`} alt='img' />
            <p>{user.username}</p>
        </div >
    )
}

export default SearchedUser;