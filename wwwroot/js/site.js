// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function showLoading(button, url = null) {
    const spinner = button.querySelector('.spinner-border');
    const text = button.querySelector('.button-text');

    spinner?.classList.remove('d-none');
    if (text) text.style.display = 'none';
    button.disabled = true;

    if (url) {
        setTimeout(() => {
            window.location.href = url;
        }, 500);
    }
    // else: let the form submit normally
}
