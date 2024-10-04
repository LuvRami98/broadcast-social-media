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
    const usernameForm = document.getElementById('usernameForm');
    const usernameError = document.getElementById('usernameError');


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




document.addEventListener('DOMContentLoaded', previewSelectedImage);
