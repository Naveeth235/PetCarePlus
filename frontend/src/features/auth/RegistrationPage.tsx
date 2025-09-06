function RegistrationPage(){
  return (
    <section className="bg-white min-h-screen flex flex-col sm:flex-row">
        <div className="hidden sm:flex w-1/2 items-center justify-end pl-4 pr-4">
            <img
                src="./Petcare_cover_image.jpg"
                alt="Login illustration"
                className="w-4/5 h-auto max-h-[70%] object-contain rounded-xl"
            />
        </div>

        <div className="w-full sm:w-1/2 flex items-center justify-center p-4 sm:p-8">
            <div className="w-full max-w-2xl bg-white rounded-xl shadow-md border p-6 sm:p-8">
                <a
                    href="#"
                    className="flex items-center mb-8 text-5xl font-bold text-gray-900"
                >
                <img
              className="w-16 h-16 mr-3"
              src="./logo.jpg"
              alt="logo"
            />
            PetCare+
          </a>

          <h1 className="text-2xl sm:text-3xl font-bold text-gray-900 mb-6">
            Create an account
          </h1>

          <form className="space-y-5">
            <div>
              <label
                htmlFor="full-name"
                className="block mb-2 text-sm font-medium text-gray-900"
              >
                Full name
              </label>
              <input
                type="text"
                id="full-name"
                placeholder="Full Name"
                className="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg
                  focus:ring-blue-600 focus:border-blue-600 block w-full p-3"
                required
              />
            </div>

            <div>
              <label
                htmlFor="email"
                className="block mb-2 text-sm font-medium text-gray-900"
              >
                Your email
              </label>
              <input
                type="email"
                id="email"
                placeholder="example@email.com"
                className="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg
                  focus:ring-blue-600 focus:border-blue-600 block w-full p-3"
                required
              />
            </div>

            <div>
              <label
                htmlFor="password"
                className="block mb-2 text-sm font-medium text-gray-900"
              >
                Password
              </label>
              <input
                type="password"
                id="password"
                placeholder="••••••••"
                className="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg
                  focus:ring-blue-600 focus:border-blue-600 block w-full p-3"
                required
              />
            </div>

            <div>
              <label
                htmlFor="confirm-password"
                className="block mb-2 text-sm font-medium text-gray-900"
              >
                Confirm password
              </label>
              <input
                type="password"
                id="confirm-password"
                placeholder="••••••••"
                className="bg-gray-50 border border-gray-300 text-gray-900 text-sm rounded-lg
                  focus:ring-blue-600 focus:border-blue-600 block w-full p-3"
                required
              />
            </div>

            <div className="flex items-start">
              <input
                id="terms"
                type="checkbox"
                className="w-4 h-4 border border-gray-300 rounded bg-gray-50 focus:ring-blue-300"
                required
              />
              <label
                htmlFor="terms"
                className="ml-2 text-sm font-light text-gray-500"
              >
                I accept the{" "}
                <a href="#" className="font-medium text-blue-600 hover:underline">
                  Terms and Conditions
                </a>
              </label>
            </div>

            <button
              type="submit"
              className="w-full text-white bg-blue-600 hover:bg-blue-700
                focus:ring-4 focus:outline-none focus:ring-blue-300 font-medium rounded-lg
                text-sm px-5 py-3 text-center"
            >
              Create an account
            </button>

            <p className="text-sm font-light text-gray-500 text-center">
              Already have an account?{" "}
              <a href="/login" className="font-medium text-blue-600 hover:underline">
                Login here
              </a>
            </p>
          </form>
        </div>
      </div>
    </section>
  );
}

export default RegistrationPage;