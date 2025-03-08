async function updateUser() {
    const formData = new FormData();

    formData.append("FirstName", document.getElementById("firstName")?.value || "");
    formData.append("LastName", document.getElementById("lastName")?.value || "");
    formData.append("Contact", document.getElementById("contact")?.value || "");
    formData.append("Bio", "");
    formData.append("NewPassword", "");
    formData.append("ConfirmPassword", "");

    for (let pair of formData.entries()) {
        console.log(pair[0], pair[1]);
    }

    try {
        const response = await fetch("/user/edit", {
            method: "POST",
            body: formData,
        });

        const result = await response.json();

        if (response.ok) {
            alert("Profile updated successfully!");
            window.location.href = "/User/Edit";
        } else {
            alert(result.error || "Failed to update user information.");
        }
    } catch (error) {
        console.error("Error:", error);
        alert("An error occurred. Please try again.");
    }
}
