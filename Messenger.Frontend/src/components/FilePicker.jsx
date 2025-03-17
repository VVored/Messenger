import React, { useEffect, useState } from 'react';
import style from './FilePicker.module.css'
import styleLogin from './Login.module.css'
import axios from 'axios'

const FilePicker = ({setAvatarUrl}) => {
    const [fileName, setFileName] = useState('Максимум 15мб');
    const [file, setFile] = useState(null);

    const uploadImage = async () => {
        const formData = new FormData();
        formData.append('file', file);
        const response = await axios.post('https://localhost:7192/api/files', formData);
        setAvatarUrl(response.data);
    }

    useEffect(() => {
        if (file) {
            uploadImage();
        }
    }, [file])

    return (
        <label className={style.input_file}>
            <input type="file" name="file" onChange={(e) => { setFileName(e.target.value); setFile(e.target.files[0]); }} />
            <span className={styleLogin.button}>Выберите файл</span>
            <span className={style.input_file_text}>{fileName}</span>
        </label>
    );
};

export default FilePicker;