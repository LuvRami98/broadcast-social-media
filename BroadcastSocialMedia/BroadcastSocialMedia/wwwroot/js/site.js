async function handleListen(userId, isListening) {
    const url = isListening ? '/Users/Listen' : '/Users/Unlisten';
    const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

    console.log('Sending request to:', url);
    console.log('CSRF Token:', csrfToken);
    console.log('UserId:', userId);

    try {
        const formData = new FormData();
        formData.append('UserId', userId);
        formData.append('__RequestVerificationToken', csrfToken);

        const response = await fetch(url, {
            method: 'POST',
            body: formData,
        });

        if (response.ok) {
            console.log('Request succeeded, reloading page.');
            location.reload();
        } else {
            console.error('Error during request:', response.status);
            alert('Failed to process the request. Please try again.');
        }
    } catch (error) {
        console.error('Error during request:', error);
    }
}

function toggleProfileUpdate() {
    var modal = document.getElementById("profileUpdateModal");
    var blurredContent = document.getElementById('blurredContent');
    var body = document.querySelector('body');

    if (modal.style.display === "flex") {
        modal.style.display = "none";
        body.classList.remove('modal-open');
        blurredContent.classList.remove("blur-background");
    } else {
        modal.style.display = "flex";
        body.classList.add('modal-open');
        blurredContent.classList.add("blur-background");
    }
}

function toggleModal() {
    var modal = document.getElementById("profileUpdateModal");
    var blurredContent = document.getElementById('blurredContent');
    var body = document.querySelector('body');

    modal.style.display = "none";
    body.classList.remove('modal-open');
    blurredContent.classList.remove("blur-background");
}

function toggleImageOptions() {
    const imageOptions = document.getElementById("imageOptions");
    imageOptions.style.display = imageOptions.style.display === "none" ? "block" : "none";
}

function toggleUpload() {
    const uploadSection = document.getElementById("uploadSection");
    uploadSection.style.display = uploadSection.style.display === "none" ? "block" : "none";
}

function toggleSelectExisting() {
    const existingSection = document.getElementById("existingSection");
    existingSection.style.display = existingSection.style.display === "none" ? "block" : "none";
}

function previewSelectedImage() {
    var radios = document.querySelectorAll('input[name="selectedImagePath"]');
    radios.forEach(radio => {
        radio.removeEventListener('change', updateProfilePicturePreview);
    });
}

function setProfilePicture() {
    const selectedImage = document.querySelector('input[name="selectedImagePath"]:checked');
    if (selectedImage) {
        const imageSrc = selectedImage.value;
        document.getElementById('profilePicturePreview').src = imageSrc;
        document.getElementById('hiddenSelectedImagePath').value = imageSrc;
    } else {
        alert("Please select an image.");
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const usernameInput = document.getElementById('Username');
    const setUsernameButton = document.getElementById('setUsernameButton');

    if (usernameInput && setUsernameButton) {
        const usernameForm = document.getElementById('usernameForm');
        const usernameError = document.getElementById('usernameError');

        if (usernameForm && usernameError) {
            usernameForm.addEventListener('submit', async function (event) {
                event.preventDefault();
                const username = usernameInput.value.trim();
                if (!username) {
                    usernameError.textContent = "Please provide a username.";
                    usernameError.style.display = 'block';
                    return;
                }
                const isTaken = await checkUsernameAvailability(username);
                if (isTaken) {
                    usernameError.textContent = "This username is already taken. Please choose another.";
                    usernameError.style.display = 'block';
                    return;
                }
                usernameError.style.display = 'none';
                usernameForm.submit();
            });

            usernameInput.addEventListener('input', function () {
                const username = usernameInput.value.trim();
                setUsernameButton.disabled = !username;
            });

            if (document.querySelector('.alert-danger')) {
                toggleProfileUpdate();
            }
        }
    }
});

async function checkUsernameAvailability(username) {
    try {
        const response = await fetch(`/Profile/CheckUsername?username=${encodeURIComponent(username)}`);
        const result = await response.json();
        return result.isTaken;
    } catch (error) {
        console.error("Error checking username availability:", error);
        return false;
    }
}

function toggleLike(broadcastId) {
    const likeButton = document.getElementById(`like-button-${broadcastId}`);
    const likeCountSpan = document.getElementById(`like-count-${broadcastId}`);

    likeButton.disabled = true;

    fetch(`/Home/ToggleLike?broadcastId=${broadcastId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(response => response.text())
        .then(likeCount => {
            console.log(`Broadcast ID: ${broadcastId}, Updated Like Count: ${likeCount}`);

            const isLiked = likeButton.classList.contains('btn-primary');

            if (isLiked) {
                likeButton.classList.remove('btn-primary');
                likeButton.classList.add('btn-secondary');
            } else {
                likeButton.classList.remove('btn-secondary');
                likeButton.classList.add('btn-primary');
            }

            likeCountSpan.textContent = likeCount;
        })
        .catch(error => console.error('Error updating like:', error))
        .finally(() => {
            likeButton.disabled = false;
        });
}

function previewProfilePicture(input) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();
        reader.onload = function (e) {
            document.getElementById('profilePicturePreview').src = e.target.result;
            document.getElementById('profilePicturePreview').style.display = 'block'; 
        }
        reader.readAsDataURL(input.files[0]);
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('broadcastForm');
    const messageInput = document.getElementById('messageInput');
    const imageInput = document.getElementById('imageInput');
    const emptyBroadcastError = document.getElementById('emptyBroadcastError');

    form.addEventListener('submit', function (event) {
        const message = messageInput.value.trim();
        const hasImage = imageInput.files.length > 0;

        if (!message && !hasImage) {
            event.preventDefault(); 
            emptyBroadcastError.textContent = "Please provide either a message or an image.";
            emptyBroadcastError.style.display = 'block';
        } else {
            emptyBroadcastError.style.display = 'none'; 
        }
    });

    form.addEventListener('input', function () {
        const canSubmit = messageInput.value.trim() || imageInput.files.length > 0;
        const submitButton = form.querySelector('input[type="submit"]');
        if (submitButton) {
            submitButton.disabled = !canSubmit; 
        }
    });
});

function toggleOptionsMenu(broadcastId) {
    const menu = document.getElementById(`options-menu-${broadcastId}`);
    if (menu.style.display === "block") {
        menu.style.display = "none";
    } else {
        menu.style.display = "block";
    }
}

function openDeleteModal(broadcastId) {
    const modal = document.getElementById(`deleteModal-${broadcastId}`);
    modal.style.display = "flex";
    document.body.classList.add('modal-open');
}

function closeDeleteModal(broadcastId) {
    const modal = document.getElementById(`deleteModal-${broadcastId}`);
    modal.style.display = "none";
    document.body.classList.remove('modal-open');
}


document.addEventListener('DOMContentLoaded', previewSelectedImage);
