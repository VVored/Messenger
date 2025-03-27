import React, { useEffect, useRef, useState } from 'react'
import axios from 'axios';
import Message from './Message';
import styles from './ChatMessages.module.css';
import styleChat from './Chat.module.css';
import leaveChatImg from '../imgs/leave_chat.png';
import FileAttachment from './FileAttachment';
import { jwtDecode } from 'jwt-decode';

const ChatMessages = ({ connection, setSelectedUser, setConnection, chat, setChats }) => {

    const [messages, setMessages] = useState([]);
    const messagesEndRef = useRef(null);
    const [currentMessage, setCurrentMessage] = useState('');
    const [isJoined, setIsJoined] = useState(false);
    const [amountOfVh, setAmountOfVh] = useState(85);
    const [files, setFiles] = useState([]);
    const [privateChatAvatarUrl, setPrivateChatAvatarUrl] = useState('');
    const [privateChatName, setPrivateChatName] = useState('');

    const scrollToBottom = () => {
        messagesEndRef.current.scrollIntoView({ behaivor: 'smooth' });
    }

    const IsUserJoined = async () => {
        const token = localStorage.getItem('token');
        const response = await axios.get(`https://localhost:7192/api/chats/${chat.chatId}/members/isjoined`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        })
        setIsJoined(response.data);
    }

    const getChatMessages = async (chatId) => {
        const token = localStorage.getItem('token');
        const response = await axios.get(`https://localhost:7192/api/messages/chats/${chatId}`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        })
        return response;
    }

    const ConfigureConnection = async () => {
        connection.on('newMessage', message => {
            if (message.chatId === chat.chatId) {
                setMessages(prev => [...prev, message]);
            }
        });
        connection.on('userJoinChat', response => {
            console.log('userJoinedChat');
            setChats(prev => [...prev, chat]);
            setIsJoined(true);
        });
        connection.on('leaveChatMember', response => {
            console.log('leaveChatMember');
            setChats(prev => prev.filter(item => item !== chat));
            setIsJoined(false);
        })
        setConnection(connection);
    }

    const sendMessage = async (e, chatId, content) => {
        if (e.key === 'Enter') {
            if (content !== '' || files.length > 0) {
                const token = localStorage.getItem('token');
                const attachments = await uploadFiles(files);
                console.log(JSON.stringify({ chatId, content, attachments }));
                await fetch('https://localhost:7192/api/messages', {
                    method: 'POST',
                    body: JSON.stringify({ chatId, content, attachments }),
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    }
                });
                setFiles([]);
                setCurrentMessage('');
                e.target.value = '';
            }
        }
    }

    const uploadFiles = async (files) => {
        let attachments = [];
        for (let i = 0; i < files.length; i++) {
            const formData = new FormData();
            formData.append('file', files[i]);
            const response = await axios.post('https://localhost:7192/api/files', formData);
            const attachment = {
                fileUrl: response.data,
                fileType: files[i].type,
                fileSize: files[i].size
            }
            attachments = [...attachments, attachment];
        }
        return attachments;
    }

    const JoinChat = async () => {
        connection.invoke('JoinChat', chat.chatId + '');
    }

    const LeaveChat = async () => {
        connection.invoke('LeaveChat', chat.chatId + '');
    }

    useEffect(() => {
        setPrivateChatAvatarUrl('');
        setPrivateChatName('');
        const token = localStorage.getItem('token');
        const decoded = jwtDecode(token);
        chat.chatMembers.forEach(chatMember => {
            if (chatMember.user.userId + '' !== decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]) {
                setPrivateChatAvatarUrl(chatMember.user.avatarUrl);
                setPrivateChatName(chatMember.user.username);
            }
        });

        IsUserJoined();
        setIsJoined(IsUserJoined());
        getChatMessages(chat.chatId).then(response => { setMessages(response.data) });
        ConfigureConnection();
    }, [chat]);

    useEffect(scrollToBottom, [messages]);

    useEffect(() => {
        files.length > 0 ? setAmountOfVh(70) : setAmountOfVh(85);
        scrollToBottom();
    }, [files]);

    return (
        <div style={{ minHeight: '100vh' }}>
            <div style={{ display: 'flex', height: '10vh', background: 'white' }}>
                <div style={{ display: 'flex', cursor: 'pointer', width: '85%', margin: 'auto 0' }}>
                    {
                        chat.chatType === 'public'
                            ? <img className={styleChat.chat_avatar} src={`https://localhost:7192/api/files/${chat.avatarUrl}`} alt='img' />
                            : <img className={styleChat.chat_avatar} src={`https://localhost:7192/api/files/${privateChatAvatarUrl}`} alt='img' />
                    }
                    {
                        chat.chatType === 'public'
                            ? <p>{chat.groupName}</p>
                            : <p>{privateChatName}</p>
                    }
                </div>
                {
                    chat.chatType === 'public'
                        ? <div style={{ display: 'flex', width: '15%' }}>
                            {isJoined ? <button className={styles.button} style={{ border: 'none' }} onClick={() => LeaveChat()}><img style={{ maxHeight: '50px' }} src={leaveChatImg} alt='' /></button> : <div></div>}
                        </div>
                        : <div></div>
                }
            </div>
            <div style={{ height: amountOfVh + 'vh', overflowY: 'auto' }}>
                {
                    messages.map(message => {
                        return <Message key={message.messageId} message={message} setSelectedUser={setSelectedUser} />
                    })
                }
                <div ref={messagesEndRef} />
            </div>
            {
                files.length > 0
                    ? <div style={{ display: 'flex', height: '15vh', backgroundColor: 'white' }}>
                        {
                            Array.from(files).map(file => {
                                return <FileAttachment key={file.name} setFiles={setFiles} file={file} />
                            })
                        }
                    </div>
                    : <div>

                    </div>
            }
            <div style={{ height: '5vh', background: 'white', margin: 'auto', borderColor: 'rgba(178, 178, 178, 0.5)', borderWidth: '1px' }}>
                {
                    isJoined
                        ? <div style={{ display: 'flex', width: '100%', height: '100%' }}>
                            <div style={{ height: '111%', width: '5%' }}>
                                <label htmlFor="file" className={styles.file_label}></label>
                                <input accept='image/png, image/gif, image/jpeg' type="file" id="file" className={styles.file_input} multiple onChange={e => { setFiles(e.target.files); }} />
                            </div>
                            <input className={styles.input} type="text" onChange={(e) => setCurrentMessage(e.target.value)} onKeyDown={(e) => sendMessage(e, chat.chatId, currentMessage)} placeholder="Напишите сообщение..." />
                        </div>
                        : <button className={styles.button} onClick={() => { JoinChat() }}>Join chat</button>}
            </div>
        </div>
    )
}

export default ChatMessages;