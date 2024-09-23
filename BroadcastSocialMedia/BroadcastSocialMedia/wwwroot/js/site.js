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
