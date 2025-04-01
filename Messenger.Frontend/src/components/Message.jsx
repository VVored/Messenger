import React from 'react';
import styles from './Message.module.css'

const Message = ({ message, setSelectedUser }) => {

    // const [isMessageMine, setIsMessageMine] = useState(false);

    // useEffect(() => {
    //     const token = localStorage.getItem('token');
    //     const decoded = jwtDecode(token);
    //     setIsMessageMine(false);
    //     if (decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] === message.sender.userId + '') {
    //         setIsMessageMine(true);
    //     }
    // }, [])

    return (
        <div style={{ display: 'flex' }}>
            <img onClick={() => { setSelectedUser(message.sender);}} className={styles.user_avatar} src={`https://localhost:7192/api/files/${message.sender.avatarUrl}`} alt='avatar'/>
            <div className={styles.message}>
                <h3 className={styles.username} onClick={() => { setSelectedUser(message.sender); }}>{message.sender.firstName} {message.sender.lastName}</h3>
                <p style={{ margin: '3px 1px' }}>{message.content}</p>
                {
                    message.attachments
                        ? message.attachments.map(attachment => {
                            return <img style={{ fontSize: '8px', width: '100%', maxWidth: '300px', maxHeight: '600px', border: '1px solid grey' }} src={`https://localhost:7192/api/files/${attachment.fileUrl}`} alt='img' key={attachment.fileUrl} />
                        })
                        : <div></div>
                }
                <p style={{ margin: '3px 1px' }} className={styles.sentAt}>{(new Date(message.sentAt)).toLocaleString()}</p>
            </div>
        </div>
    )
}

export default Message;