const uri = 'api/Comments';
let comments = [];

function getComments() {
    fetch(uri)
        .then(response => response.json())
        .then(data => _displayComments(data))
        .catch(error => console.error('Unable to get comments.', error));
}

function addComment() {
    const contentTextbox = document.getElementById('add-content');
    const userIdTextbox = document.getElementById('add-userId');
    const taskIdTextbox = document.getElementById('add-taskId');
    const taskSubmissionIdTextbox = document.getElementById('add-taskSubmissionId');

    const comment = {
        content: contentTextbox.value.trim(),
        userId: parseInt(userIdTextbox.value),
        taskId: taskIdTextbox.value ? parseInt(taskIdTextbox.value) : null,
        taskSubmissionId: taskSubmissionIdTextbox.value ? parseInt(taskSubmissionIdTextbox.value) : null
    };

    fetch(uri, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(comment)
    })
        .then(response => {
            if (!response.ok) {
                return response.json().then(err => { throw new Error(err.message); });
            }
            return response.json();
        })
        .then(() => {
            getComments();
            contentTextbox.value = '';
            userIdTextbox.value = '';
            taskIdTextbox.value = '';
            taskSubmissionIdTextbox.value = '';
        })
        .catch(error => console.error('Unable to add comment.', error));
}

function deleteComment(id) {
    fetch(`${uri}/${id}`, {
        method: 'DELETE'
    })
        .then(() => getComments())
        .catch(error => console.error('Unable to delete comment.', error));
}

function displayEditForm(id) {
    const comment = comments.find(comment => comment.id === id);

    document.getElementById('edit-id').value = comment.id;
    document.getElementById('edit-content').value = comment.content;
    document.getElementById('edit-userId').value = comment.userId;
    document.getElementById('edit-taskId').value = comment.taskId || '';
    document.getElementById('edit-taskSubmissionId').value = comment.taskSubmissionId || '';
    document.getElementById('editComment').style.display = 'block';
}

function updateComment() {
    const commentId = document.getElementById('edit-id').value;
    const comment = {
        id: parseInt(commentId, 10),
        content: document.getElementById('edit-content').value.trim(),
        userId: parseInt(document.getElementById('edit-userId').value),
        taskId: document.getElementById('edit-taskId').value ? parseInt(document.getElementById('edit-taskId').value) : null,
        taskSubmissionId: document.getElementById('edit-taskSubmissionId').value ? parseInt(document.getElementById('edit-taskSubmissionId').value) : null
    };

    fetch(`${uri}/${commentId}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(comment)
    })
        .then(() => getComments())
        .catch(error => console.error('Unable to update comment.', error));

    closeInput();
    return false;
}

function closeInput() {
    document.getElementById('editComment').style.display = 'none';
}

function _displayComments(data) {
    const tBody = document.getElementById('comments');
    tBody.innerHTML = '';

    const button = document.createElement('button');

    data.forEach(comment => {
        let editButton = button.cloneNode(false);
        editButton.innerText = 'Edit';
        editButton.setAttribute('onclick', `displayEditForm(${comment.id})`);

        let deleteButton = button.cloneNode(false);
        deleteButton.innerText = 'Delete';
        deleteButton.setAttribute('onclick', `deleteComment(${comment.id})`);

        let tr = tBody.insertRow();

        let td1 = tr.insertCell(0);
        let textNode = document.createTextNode(comment.content);
        td1.appendChild(textNode);

        let td2 = tr.insertCell(1);
        let userIdNode = document.createTextNode(comment.userId);
        td2.appendChild(userIdNode);

        let td3 = tr.insertCell(2);
        let taskIdNode = document.createTextNode(comment.taskId || 'N/A');
        td3.appendChild(taskIdNode);

        let td4 = tr.insertCell(3);
        let taskSubmissionIdNode = document.createTextNode(comment.taskSubmissionId || 'N/A');
        td4.appendChild(taskSubmissionIdNode);

        let td5 = tr.insertCell(4);
        let createdAtNode = document.createTextNode(new Date(comment.createdAt).toLocaleString());
        td5.appendChild(createdAtNode);

        let td6 = tr.insertCell(5);
        td6.appendChild(editButton);
        td6.appendChild(deleteButton);
    });

    comments = data;
}