import React from 'react';
import styles from './Message.module.css'

const Message = ({ message, setSelectedUser }) => {

    return (
        <div style={{display: 'flex'}}>
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