        // Countdown Timer
    function updateTimer() {
            // Set target date (30 days from now for demo)
            const targetDate = new Date();
    targetDate.setDate(targetDate.getDate() + 29);
    targetDate.setHours(targetDate.getHours() + 15);
    targetDate.setMinutes(targetDate.getMinutes() + 42);

    function update() {
                const now = new Date();
    const diff = targetDate - now;

                if (diff > 0) {
                    const days = Math.floor(diff / (1000 * 60 * 60 * 24));
    const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
    const seconds = Math.floor((diff % (1000 * 60)) / 1000);

    document.getElementById('days').textContent = days.toString().padStart(2, '0');
    document.getElementById('hours').textContent = hours.toString().padStart(2, '0');
    document.getElementById('minutes').textContent = minutes.toString().padStart(2, '0');
    document.getElementById('seconds').textContent = seconds.toString().padStart(2, '0');
                }
            }

    update();
    setInterval(update, 1000);
        }

    // Category Form Handler
    document.getElementById('categoryForm').addEventListener('submit', function(e) {
        e.preventDefault();

    const name = document.getElementById('categoryName').value;
    const desc = document.getElementById('categoryDescription').value;

    if (name && desc) {
                const container = document.getElementById('categoryContainer');
    const categoryItem = document.createElement('div');
    categoryItem.className = 'category-item mb-2 p-3 rounded';
    categoryItem.innerHTML = `
    <strong>${name}</strong>
    <p class="mb-0 small text-muted">${desc}</p>
    `;
    container.appendChild(categoryItem);

    // Show success message
    const msg = document.getElementById('addMessage');
    msg.innerHTML = '<div class="alert alert-success py-2">Category added successfully!</div>';
                setTimeout(() => msg.innerHTML = '', 3000);

    // Reset form
    this.reset();
            }
        });

    // Upload Course Form Handler
    document.getElementById('uploadCourseForm').addEventListener('submit', function(e) {
        e.preventDefault();
    alert('Course uploaded successfully!');
    this.reset();
        });

        // Smooth Scroll for Navigation
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
        });

    // Active Nav Link on Scroll
    window.addEventListener('scroll', function() {
            const sections = document.querySelectorAll('section[id]');
    const navLinks = document.querySelectorAll('.nav-link');

    let current = '';
            sections.forEach(section => {
                const sectionTop = section.offsetTop - 100;
                if (scrollY >= sectionTop) {
        current = section.getAttribute('id');
                }
            });

            navLinks.forEach(link => {
        link.classList.remove('active');
    if (link.getAttribute('href') === '#' + current) {
        link.classList.add('active');
                }
            });
        });

    // Initialize
    document.addEventListener('DOMContentLoaded', function() {
        updateTimer();
        });



