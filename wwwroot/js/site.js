// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// function login() {
//         document.getElementById("login-text").textContent="incorrect username or password"
// }

function strcmp(a, b)
{   
    return (a<b?-1:(a>b?1:0));  
}

function signup() {
        const password = document.getElementById("password").value;
        const confirm_password = document.getElementById("confirm-password").value;

        if (password === confirm_password) {
                document.getElementById("error-text").textContent = ""
                document.getElementById("form").submit()
        }
        else {
                document.getElementById("error-text").textContent = "unmatch password"
        }

}

