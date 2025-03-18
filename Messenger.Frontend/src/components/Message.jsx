import React from 'react';
import styles from './Message.module.css'
const Message = ({ message }) => {

    return (
        <div className={styles.message}>
            <h3 className={styles.username}>{message.sender.firstName} {message.sender.lastName}</h3>
            <p style={{ margin: '3px 1px' }}>{message.content}</p>
            <p style={{ margin: '3px 1px' }} className={styles.sentAt}>{(new Date(message.sentAt)).toLocaleString()}</p>
            {
                message.attachments
                    ? message.attachments.map(attachment => {
                        return <p style={{fontSize: '8px'}} key={attachment.fileUrl}>{attachment.fileUrl}</p>
                    })
                    : <div></div>
            }
        </div>
    )
}

export default Message;