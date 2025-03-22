import React from 'react';

const FileAttachment = ({ setFiles, file }) => {
    
    const removeFile = (file) => {
        setFiles(prev => Array.from(prev).filter(item => item !== file));
    }

    return (
        <div style={{padding: '10px'}}>
            <img height='100%' src={URL.createObjectURL(file)} alt={file.name}/>
            <button onClick={() => {removeFile(file)}} style={{verticalAlign: 'top', border: '1px solid red', borderRadius: '100%', backgroundColor: 'red', color: 'white', height: '25%', margin: 'auto', cursor: 'pointer'}}>×</button>
        </div>
    )
}

export default FileAttachment;