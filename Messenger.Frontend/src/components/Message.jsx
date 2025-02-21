import React from 'react';
const Message = ({ message }) => {

    return (
        <div>
            <p>{message.content}</p>
        </div>
    )
}

export default Message;