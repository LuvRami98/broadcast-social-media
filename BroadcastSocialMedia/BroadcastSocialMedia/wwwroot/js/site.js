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

document.addEventListener('DOMContentLoaded', previewSelectedImage);